using CreativeBudgeting.Models;
using CreativeBudgeting.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CreativeBudgeting.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaychecksController(BudgetDbContext context) : ControllerBase
    {
        private readonly BudgetDbContext _context = context;

        // GET: api/Paychecks/{userId}
        [HttpGet("{userId}")]
        public async Task<ActionResult<List<PayCheckDto>>> GetPaychecksByUserId(int userId)
        {
            var paychecks = await _context.Paychecks.Include(p => p.Expenses)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PayDate)
                .Select(p => new PayCheckDto
                {
                    Id = p.Id,
                    PayDate = p.PayDate,
                    Amount = p.Amount,
                    
                   
                })
                .ToListAsync();

           

            return Ok(paychecks);
        }
        [HttpGet("{userId}/budget")]
        public async Task<IActionResult> GetPaychecksForBudget([FromRoute] int userId)
        {
            var paychecks = await _context.Paychecks
                .Where(p => p.UserId == userId)
                .Include(p => p.Expenses)
                .ToListAsync();

            return Ok(paychecks);
        }
        [HttpPost("{userId}")]
        public async Task<ActionResult<Paycheck>> PostPaycheck(int userId, [FromBody] PayCheckDto dto)
        {
            // Validate if the user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return BadRequest("User does not exist.");
            }

            // Create a new Paycheck
            var paycheck = new Paycheck
            {
                PayDate = dto.PayDate.ToUniversalTime(),
                Amount = dto.Amount,
                UserId = userId
                
            };

            

            // Add the paycheck to the context and save
            _context.Paychecks.Add(paycheck);
            await _context.SaveChangesAsync();

            // Return the created paycheck (with expenses if any)
            return Ok(paycheck);
        }

        // PUT: api/Paychecks/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPaycheck(int id, PayCheckDto dto)
        {
            var existingPaycheck = await _context.Paychecks.FirstOrDefaultAsync(p => p.Id == id);
            if (existingPaycheck == null)
            {
                return NotFound();
            }

           
            existingPaycheck.PayDate = dto.PayDate;
            existingPaycheck.Amount = dto.Amount;
            await _context.SaveChangesAsync();
            return Ok(existingPaycheck);
        }

        // DELETE: api/Paychecks/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaycheck(int id)
        {
            var paycheck = await _context.Paychecks.FirstOrDefaultAsync(p => p.Id == id);
            if (paycheck == null)
            {
                return NotFound();
            }

            _context.Paychecks.Remove(paycheck);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
