using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CreativeBudgeting.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CreativeBudgeting.Controllers
{
    [ApiController]
    [Route("api/account")]  // Ensure this is prefixed with api/account
    public class AccountController : ControllerBase
    {
        private readonly BudgetDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();

        public AccountController(BudgetDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            // Find the user by username
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == model.Username);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            // Ensure the password hasher is properly used
            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.Hash, model.Password);

            if (passwordVerificationResult != PasswordVerificationResult.Success)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            // Success — return user info (you might eventually want to return a JWT or token here)
            return Ok(new
            {
                userId = user.Id,
                username = user.Username
            });
        }
        


    }
}
