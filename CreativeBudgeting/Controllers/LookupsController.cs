using System.Threading.Tasks;
using CreativeBudgeting.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CreativeBudgeting.Controllers
{
   
        [ApiController]
        [Route("api/[controller]")]
        public class  LookupsController: ControllerBase 
        {
        private readonly BudgetDbContext _context;

        public LookupsController(BudgetDbContext context)
        {
            _context = context;
        }
        [HttpGet("ticket-severities")]
        public async Task<IActionResult> GetTicketSeverities()
        {
            var query = await _context.TicketSeverities.FirstOrDefaultAsync();

            var severities = await _context.TicketSeverities
                .Select(s => new TicketSeverityDto
                {
                    Id = s.Id,
                    Value = s.Value
                })
                .ToListAsync();
            return Ok(severities);
        }
    }
    
}
