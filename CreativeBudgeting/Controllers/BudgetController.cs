using System.Collections;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using CreativeBudgeting.Models;
using CreativeBudgeting.Services;
using CreativeBudgeting.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace CreativeBudgeting.Controllers
{
    [ApiController]
    //[Authorize]
    [Route("api/[controller]")]
    public class BudgetController : ControllerBase
    {
        private readonly BudgetDbContext _context;
        private readonly PasswordService _passwordService;

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

        public BudgetController(BudgetDbContext context, PasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        [HttpGet("health")]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                // Test database connection
                await _context.Database.CanConnectAsync();
                
                return Ok(new
                {
                    status = "Healthy",
                    timestamp = DateTime.UtcNow,
                    database = "Connected",
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                    version = "1.0.0"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new
                {
                    status = "Unhealthy",
                    timestamp = DateTime.UtcNow,
                    database = "Disconnected",
                    error = ex.Message
                });
            }
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

        [HttpPost("expenses/{userId}")]
        public async Task<ActionResult<Expense>> AddExpenses(
    [FromRoute] int userId,
    [FromBody] AddExpensesDto dto
)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
            {
                return NotFound("User not found.");
            }
           

            var expense = new Expense
            {
                Name = dto.Name,
                Payment = dto.Payment,
                TotalBalance = dto.TotalBalance,
                DueDate = dto.DueDate,
                UserId = dto.UserId,
                CategoryId = dto.CategoryId,
                SubcategoryId = dto.SubcategoryId ?? 66,
                PaycheckId = dto.PaycheckId
            };

            if (expense.CategoryId == 10)
            {
                expense.Name = "Savings";
                var savings = await _context.Savings
                    .FirstOrDefaultAsync(s => s.UserId == expense.UserId);

                if (savings == null)
                {
                    savings = new Savings
                    {
                        Amount = expense.Payment,
                        UserId = expense.UserId
                    };
                    await _context.Savings.AddAsync(savings);
                }
                else
                {
                    savings.Amount += expense.Payment;
                    _context.Savings.Update(savings);
                }
            }

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
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
            var expense = await _context.Expenses.FirstOrDefaultAsync(e => e.Id == id);

            if (expense == null)
            {
                return NotFound("Expense not found.");
            }

            var prevBalance = expense.TotalBalance ?? 0;
            var paymentTowardsBalance = expense.Payment;
            var newBalance = prevBalance - paymentTowardsBalance;

            expense.TotalBalance = newBalance;
            expense.IsPaid = dto.IsPaid;

            _context.Expenses.Update(expense);
            await _context.SaveChangesAsync();

            return Ok(new { newBalance, expense });
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
                    SubcategoryId = expense.SubcategoryId ?? 0,
                    SubcategoryName = expense.Subcategory != null ? expense.Subcategory.Name : null,
                    IsPaid = expense.IsPaid,
                    TotalBalance = expense.TotalBalance, 
                    PaycheckId = expense.PaycheckId
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
                TotalBalance = expense.TotalBalance ?? 0,
                SubcategoryId = expense.SubcategoryId ?? 0,
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
        [HttpGet("{userId}/savings")]
        public async Task<ActionResult<SavingsDto>> GetSavingsAmount(int userId)
        {
            var savings = await _context
                .Savings.Where(s => s.UserId == userId)
                .Select(s => new SavingsDto
                {
                    Id = s.Id,
                    Amount = s.Amount,
                    UserId = s.UserId

                })
                .FirstOrDefaultAsync();
            return Ok(savings);
        }
        [HttpPost("savings/{userId}")]
        public async Task<ActionResult<SavingsDto>> AddSavingsAmount(int userId, [FromBody] SavingsDto dto)
        {
            var newAmount = dto.Amount;

            var savings = await _context.Savings
                .Where(s => s.UserId == userId)
                .FirstOrDefaultAsync();

            
            if (savings == null)
            {
                savings = new Savings
                {
                    UserId = userId,
                    Amount = newAmount
                };

                await _context.Savings.AddAsync(savings);
            }
            else
            {
                savings.Amount += newAmount;
                _context.Savings.Update(savings);

            }

           

            await _context.SaveChangesAsync();

            var savingsDto = new SavingsDto
            {
                Id = savings.Id,
                Amount = savings.Amount,
                UserId = savings.UserId
            };

            return Ok(savingsDto); // Return full SavingsDto
        }

        [HttpPut("savings/{userId}")]
        public async Task<ActionResult<SavingsDto>> DeductFromSavings(int userId, [FromBody] SavingsDto dto)
        {
            var amountToDeduct = dto.Amount;
            var savings = await _context.Savings.Where(s => s.UserId == userId).FirstOrDefaultAsync();
            if(savings == null)
            {
                return BadRequest("You can't remove from an non existing saving.");
            }
            if(savings.Amount <= 0)
            {
                return BadRequest("You can't deduct savings lower than zero.");
            }
            savings.Amount -= amountToDeduct;
            _context.Savings.Update(savings);
            await _context.SaveChangesAsync();
            var savingsDto = new SavingsDto
            {
                Id = savings.Id,
                Amount = savings.Amount,
                UserId = savings.UserId
            };
            return Ok(savingsDto);
        }
    }


}