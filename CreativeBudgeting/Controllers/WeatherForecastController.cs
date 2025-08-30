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
            try
            {
                Console.WriteLine("Login attempt started");
                
                // Validate input
                if (model == null || string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
                {
                    Console.WriteLine("Login failed: Missing username or password");
                    return BadRequest(new { message = "Username and password are required" });
                }

                Console.WriteLine($"Login attempt for username: {model.Username}");

                // Test database connection first
                var canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    Console.WriteLine("Login failed: Cannot connect to database");
                    return StatusCode(500, new { message = "Database connection error. Please try again." });
                }

                Console.WriteLine("Database connection verified for login");

                // Find the user by username
                var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == model.Username);
                if (user == null)
                {
                    Console.WriteLine($"Login failed: User '{model.Username}' not found");
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                Console.WriteLine($"User found: {user.Username}");

                // Ensure the password hasher is properly used
                var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.Hash, model.Password);

                if (passwordVerificationResult != PasswordVerificationResult.Success)
                {
                    Console.WriteLine($"Login failed: Password verification failed for user '{model.Username}'");
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                Console.WriteLine($"Login successful for user: {user.Username}");

                // Success — return user info (you might eventually want to return a JWT or token here)
                return Ok(new
                {
                    userId = user.Id,
                    username = user.Username
                });
            }
            catch (Exception ex)
            {
                // Log detailed error information
                Console.WriteLine($"Login error: {ex.Message}");
                Console.WriteLine($"Login error type: {ex.GetType().Name}");
                Console.WriteLine($"Login error stack trace: {ex.StackTrace}");
                
                return StatusCode(500, new { message = "An error occurred during login. Please try again." });
            }
        }



    }
}
