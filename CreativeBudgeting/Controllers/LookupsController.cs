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
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { controller = "LookupsController", status = "healthy", timestamp = DateTime.UtcNow });
        }

        [HttpGet("ticket-severities")]
        public async Task<IActionResult> GetTicketSeverities()
        {
            try
            {
                var severities = await _context.TicketSeverities
                    .Select(s => new TicketSeverityDto
                    {
                        Id = s.Id,
                        Value = s.Value
                    })
                    .OrderBy(s => s.Id) // Order by ID for consistent ordering
                    .ToListAsync();

                Console.WriteLine($"Retrieved {severities.Count} ticket severities");
                return Ok(severities);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving ticket severities: {ex.Message}");
                return StatusCode(500, new { message = "Failed to retrieve ticket severities", error = ex.Message });
            }
        }
    }
    
}
