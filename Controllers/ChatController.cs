using AutheticationAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutheticationAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : Controller
    {
        private readonly AppDbContext _context;

        public ChatController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("GetChatList")]
        public async Task<ActionResult<IEnumerable<UserChatListDTO>>> GetChatList()
        {
            var users = await _context.Users
                .Include(u => u.UserStatus) // Assuming navigation property exists
                .Select(u => new UserChatListDTO
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    UserName = u.Username,
                    LastName = u.LastName,
                    PhoneNumber = u.PhoneNumber,
                    IsOnline = u.UserStatus != null && u.UserStatus.IsOnline,
                    LastSeen = u.UserStatus != null ? u.UserStatus.LastSeen : null
                })
                .ToListAsync();

            if (users == null || users.Count == 0)
            {
                return NotFound("No users found");
            }

            return Ok(users);
        }

        [HttpGet("GetChatHistory/{receiverId}")]
        public async Task<IActionResult> GetChatHistory(Guid receiverId)
        {
            // Get logged-in user ID from JWT claims
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            Guid currentUserId = Guid.Parse(userIdClaim.Value);

            // Fetch chat history between logged-in user and the selected user
            var chatHistory = await _context.Messages
                .Where(m =>
                    (m.SenderId == currentUserId && m.RecipientId == receiverId) ||
                    (m.SenderId == receiverId && m.RecipientId == currentUserId))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            return Ok(chatHistory);
        }

        public class Message
        {
            public Guid Id { get; set; }
            public Guid SenderId { get; set; }
            public Guid RecipientId { get; set; }
            public string Content { get; set; }
            public DateTime Timestamp { get; set; }
            public bool IsRead { get; set; }
        }

        public class UserChatListDTO
        {
            public Guid Id { get; set; }

            public string UserName { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string PhoneNumber { get; set; }
            public bool IsOnline { get; set; }
            public DateTime? LastSeen { get; set; }
        }
    }
}
