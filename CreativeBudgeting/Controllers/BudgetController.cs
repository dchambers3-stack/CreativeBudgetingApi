using System.Net;
using System.Text.RegularExpressions;
using CreativeBudgeting.Models;
using CreativeBudgeting.Models.Seeds;
using CreativeBudgeting.Services;
using CreativeBudgeting.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CreativeBudgeting.Controllers
{
    [ApiController]
    //[Authorize]
    [Route("api/[controller]")]
    public class BudgetController : ControllerBase
    {
        private readonly BudgetDbContext _context;
        private readonly PasswordService _passwordService;
        private readonly RecurringService _recurringService;

        private bool IsValidPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                return false;

            // Regex to match at least one special character (non-alphanumeric)
            var specialCharRegex = new Regex(@"[!@#$%^&*(),.?""':{}|<>]");
            // Regex to match at least one uppercase letter
            var uppercaseRegex = new Regex(@"[A-Z]");

            return specialCharRegex.IsMatch(password) && uppercaseRegex.IsMatch(password);
        }

        public BudgetController(
            BudgetDbContext context,
            PasswordService passwordService,
            RecurringService recurringService
        )
        {
            _context = context;
            _passwordService = passwordService;
            _recurringService = recurringService;
        }

        [HttpGet("users")]
        public IActionResult GetUsers()
        {
            var users = _context
                .Users.Select(u => new GetUserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Hash = u.Hash,
                })
                .ToList();
            return Ok(users);
        }

        [HttpPost("user")]
        public async Task<IActionResult> CreateUser([FromBody] AddUserDto dto)
        {
            if (dto.Hash?.Length < 8 || !IsValidPassword(dto.Hash))
            {
                return BadRequest(
                    "Password must be at least 8 characters long, have at least one special character, and one uppercase letter."
                );
            }
            var hashedPassword = _passwordService.HashPassword(dto.Hash);
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                Hash = hashedPassword,
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, user);
        }

        [HttpPost("{id}/profile-picture")]
        public async Task<IActionResult> UploadProfilePicture(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            using (var memoryStream = new MemoryStream()) // new instance of MemoryStream to hold the file data
            {
                await file.CopyToAsync(memoryStream); // copies the contents of the uploaded file into the memory stream
                user.ProfilePicture = memoryStream.ToArray(); // writes stream contents into a byte array
                user.ProfilePictureContentType = file.ContentType; // gets the raw Content-Type header of the uploaded file
            }
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Profile picture uploaded successfully." });
        }

        [HttpGet("{id}/profile-picture")]
        public async Task<IActionResult> GetProfilePicture(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || user.ProfilePicture == null)
                return NotFound();

            return File(user.ProfilePicture, user.ProfilePictureContentType ?? "image/jpeg");
        }

        [HttpPut("profile-picture/{id}")]
        public async Task<IActionResult> EditProfilePicture(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                user.ProfilePicture = memoryStream.ToArray();
                user.ProfilePictureContentType = file.ContentType;
            }
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Picture updated successfully." });
        }

        [HttpDelete("user/{id}")]
        public async Task<IActionResult> RemoveUser([FromRoute] int id)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (existingUser != null)
            {
                _context.Users.Remove(existingUser);
            }
            await _context.SaveChangesAsync();
            return Ok("Message deleted successfully.");
        }

        [HttpGet("dashboard-info/{userId}")]
        public async Task<IActionResult> GetDashboardInfo([FromRoute] int userId)
        {
            var personal = await _context.PersonalInfo.FirstOrDefaultAsync(u => u.UserId == userId);

            var dashboardData = new DashboardDto
            {
                UserId = userId,

                FirstName = personal?.FirstName,
            };

            return Ok(dashboardData);
        }

        [HttpPost("personal-info/{userId}")]
        public async Task<ActionResult<PersonalInfo>> AddPersonalInfo(
            int userId,
            [FromBody] AddPersonalInfoDto dto
        )
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Create new PersonalInfo and associate it with the User
            var personalInfo = new PersonalInfo
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                UserId = userId, // Associate the PersonalInfo with the UserId
            };

            _context.PersonalInfo.Add(personalInfo);
            await _context.SaveChangesAsync();

            return Ok(personalInfo);
        }

        [HttpPatch("personal-info/{userId}")]
        public async Task<ActionResult<PersonalInfo>> EditPersonalInfo(
            int userId,
            [FromBody] AddPersonalInfoDto dto
        )
        {
            var existingPay = await _context.PersonalInfo.FirstOrDefaultAsync(p =>
                p.UserId == userId
            );

            // Update existing
            if (existingPay != null)
            {
                existingPay.FirstName = dto.FirstName;
                existingPay.LastName = dto.LastName;
            }

            await _context.SaveChangesAsync();

            return Ok(existingPay);
        }

        [HttpPost("expenses/{paycheckId}")]
        public async Task<ActionResult<Expense>> AddExpenses(
            int paycheckId,
            [FromBody] AddExpensesDto dto
        )
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            var paycheckExists = await _context.Paychecks.AnyAsync(p => p.Id == paycheckId);

            if (!paycheckExists)
            {
                return BadRequest("Invalid PaycheckId.");
            }

            var expense = new Expense
            {
                Id = dto.Id,
                Name = dto.Name,
                Payment = dto.Payment,
                DueDate = dto.DueDate,
                UserId = dto.UserId,
                CategoryId = dto.CategoryId,
                SubcategoryId = dto.SubcategoryId,
                PaycheckId = paycheckId,
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            return Ok(expense);
        }

        [HttpPut("expenses/{userId}")]
        public async Task<ActionResult<Expense>> EditExpense(
            int userId,
            [FromBody] AddExpensesDto dto
        )
        {
            var existingExpense = await _context.Expenses.FirstOrDefaultAsync(e =>
                e.UserId == userId
            );

            if (existingExpense != null)
            {
                existingExpense.Name = dto.Name;
                existingExpense.Payment = dto.Payment;
                existingExpense.DueDate = dto.DueDate;
                existingExpense.UserId = dto.UserId;
                existingExpense.CategoryId = dto.CategoryId;
                existingExpense.SubcategoryId = dto.SubcategoryId;
                existingExpense.IsPaid = dto.IsPaid;
            }

            await _context.SaveChangesAsync();

            return Ok(existingExpense);
        }

        [HttpPatch("expenses/{id}/status")]
        public async Task<IActionResult> UpdateExpenseStatus(
            int id,
            [FromBody] MarkExpenseAsPaidDto dto
        )
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
                return NotFound();

            expense.IsPaid = dto.IsPaid;
            await _context.SaveChangesAsync();

            return Ok(expense);
        }

        [HttpGet("expenses/{userId}")]
        public async Task<IActionResult> GetExpenses([FromRoute] int userId)
        {
            var expenses = await _context
                .Expenses.Include(e => e.Category)
                .Include(e => e.Subcategory)
                .Where(e => e.UserId == userId)
                .ToListAsync();

            if (expenses == null || expenses.Count == 0)
            {
                return NotFound("No expenses found for the specified user.");
            }

            var expenseDtos = expenses
                .Select(expense => new ExpenseResponseDto
                {
                    Id = expense.Id,
                    UserId = expense.UserId,
                    Name = expense.Name,
                    Payment = expense.Payment,
                    DueDate = expense.DueDate,
                    CategoryId = expense.CategoryId,
                    CategoryName = expense.Category != null ? expense.Category.Name : null,
                    SubcategoryId = expense.SubcategoryId,
                    SubcategoryName = expense.Subcategory != null ? expense.Subcategory.Name : null,
                    IsPaid = expense.IsPaid,
                })
                .ToList();

            return Ok(expenseDtos);
        }

        [HttpDelete("expenses/{id}")]
        public async Task<IActionResult> DeleteExpense([FromRoute] int id)
        {
            var expense = await _context.Expenses.FirstOrDefaultAsync(e => e.Id == id);
            if (expense == null)
            {
                return NotFound();
            }

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Get expense by id
        [HttpGet("expense/{id}")]
        public async Task<IActionResult> GetExpense([FromRoute] int id)
        {
            var expense = await _context.Expenses.FirstOrDefaultAsync(e => e.Id == id);

            if (expense == null)
            {
                return NotFound($"Expense with ID {id} not found");
            }
            var expenseDto = new AddExpensesDto
            {
                Id = expense.Id,
                CategoryId = expense.CategoryId,
                DueDate = expense.DueDate,
                IsPaid = expense.IsPaid,
                Name = expense.Name,
                Payment = expense.Payment,
                SubcategoryId = expense.SubcategoryId,
            };
            return Ok(expenseDto);
        }

        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            var categories = await _context.Categories.ToListAsync();
            return Ok(categories);
        }

        [HttpGet("subcategories/{categoryId}")]
        public async Task<ActionResult<IEnumerable<Subcategory>>> GetSubcategories(int categoryId)
        {
            var subcategories = await _context
                .Subcategories.Where(sc => sc.CategoryId == categoryId) // Filter by categoryId
                .ToListAsync();

            if (subcategories == null || subcategories.Count == 0)
            {
                return NotFound("No subcategories found for the specified category.");
            }
            return Ok(subcategories);
        }

        //[HttpGet("Subcategories/{userId}")]
        //public async Task<IActionResult> GetSubcategories([FromRoute]int userId)
        //{
        //    var content = await _context.Expenses.Where(e => e.UserId == userId).Include(e => e.Subcategory).ToListAsync();
        //    return Ok(content);
        //}
        [HttpGet("expenses/paycheck/{paycheckId}")]
        public async Task<ActionResult<IEnumerable<Expense>>> GetExpensesByPaycheck(int paycheckId)
        {
            var expenses = await _context
                .Expenses.Where(e => e.PaycheckId == paycheckId)
                .ToListAsync();

            if (expenses == null)
            {
                return NotFound();
            }

            return Ok(expenses);
        }

        //// POST: api/expenses
        //[HttpPost("expenses")]
        //public async Task<ActionResult<Expense>> CreateExpense(Expense expense)
        //{
        //    _context.Expenses.Add(expense);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction(nameof(GetExpensesByPaycheck), new { paycheckId = expense.PaycheckId }, expense);
        //}

        [HttpPost("expenses/{userId}/recurring-expense")]
        public async Task<ActionResult<RecurringExpenseDto>> AddRecurringExpense(
            [FromBody] RecurringExpenseDto dto,
            [FromRoute] int userId
        )
        {
            var recurringExpense = new RecurringExpense
            {
                Id = new Guid(),
                RecurringExpenseName = dto.RecurringExpenseName ?? string.Empty,
                RecurringAmount = dto.RecurringAmount,
                FrequencyId = dto.FrequencyId,
                UserId = userId,
                PaycheckId = dto.PaycheckId,
                CategoryId = dto.CategoryId,
                SubcategoryId = dto.SubcategoryId,
            };
            _context.RecurringExpenses.Add(recurringExpense);
            await _context.SaveChangesAsync();
            await _recurringService.GenerateMonthlyRecurringExpensesAsync();
            return Ok(recurringExpense.Id);
        }

        [HttpGet("expenses/{userId}/recurring-expenses")]
        public async Task<ActionResult<IEnumerable<RecurringExpenseDto>>> GetRecurringExpenes(
            [FromRoute] int userId
        )
        {
            var recurringExpenses = await _context
                .RecurringExpenses.Where(re => re.UserId == userId)
                .Include(re => re.Frequency)
                .Select(re => new RecurringExpenseDto
                {
                    Id = re.Id,
                    RecurringExpenseName = re.RecurringExpenseName,
                    RecurringAmount = re.RecurringAmount,
                    UserId = re.UserId,
                    FrequencyId = re.FrequencyId,
                })
                .ToListAsync();

            return Ok(recurringExpenses);
        }

        [HttpDelete("recurring-expense/{id}")]
        public async Task<ActionResult<int>> DeleteRecurringExpense([FromRoute] Guid id)
        {
            var recurringExpense = await _context.RecurringExpenses.FirstOrDefaultAsync(re =>
                re.Id == id
            );
            if (recurringExpense == null)
            {
                return NotFound("Expense couldn't be found.");
            }
            _context.RecurringExpenses.Remove(recurringExpense);
            await _context.SaveChangesAsync();
            return Ok(recurringExpense.Id);
        }
    }
}
