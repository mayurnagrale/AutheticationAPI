using AutheticationAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutheticationAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserStatusController : Controller
    {
        private readonly AppDbContext _context;

        public UserStatusController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet] //  e.g., /api/userstatus (no specific action name in the route)
                  //[AllowAnonymous] //If you dont want to Authorize
        [Authorize]
        public async Task<ActionResult<IEnumerable<object>>> GetUserStatusList()
        {
            var users = await _context.Users
                .Include(u => u.UserStatus)
                .Select(u => new {
                    Username = u.Username,
                    IsOnline = u.UserStatus != null ? u.UserStatus.IsOnline : false,
                    LastSeen = u.UserStatus != null ? u.UserStatus.LastSeen : (DateTime?)null
                })
                .ToListAsync();

            return Ok(users);
        }
    }
}
