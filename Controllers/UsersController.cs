using AutheticationAPI.Data;
using AutheticationAPI.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutheticationAPI.Controllers
{
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;
        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("getidbyusername")]
        // [Authorize] // Optional, depending on your requirements
        public async Task<ActionResult<UserDto>> GetUserIdByUsername([FromQuery] string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return NotFound(); // Or a more appropriate error response
            }
            var userDto = new UserDto
            {
                UserId = user.Id,        // Map to Dto
                Username = user.Username
            };

            return Ok(userDto);
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
