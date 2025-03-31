using AutheticationAPI.Data;
using AutheticationAPI.Entities;
using AutheticationAPI.Hubs;
using AutheticationAPI.Models;
using AutheticationAPI.Services;
using AutheticationAPI.Shared.Utility;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AutheticationAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController: ControllerBase
    {
        private static User user = new();
        private readonly IAuthService _authService;
        private readonly IUserStatusService _userStatusService;
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ConnectionManager _connectionManager;


        public AuthController(ConnectionManager connectionManager, IAuthService authService, AppDbContext context, IHubContext<NotificationHub> hubContext, IUserStatusService userStatusService)
        {
            _authService = authService;
            _context = context;
            _hubContext = hubContext;
            _userStatusService = userStatusService;
            _connectionManager = connectionManager;
        }

        [HttpPost("Register")]
        public async Task<ActionResult<User>> Register(UserModel request)
        {
            var user = await _authService.RegisterAsync(request);
            if (user == null) {
                return BadRequest("User is already exists");
            } 

            return Ok(user);
        }

        [HttpPost("Login")]
        public async Task<ActionResult<TokenResponseDto>> Login(UserModel request)
        {
            var result = await _authService.LoginAsync(request);
            if (result == null) {
                return BadRequest("Invalid username or password");
            }

            var user = await _context.Users.FindAsync(result.UserId);

            if (user == null)
            {
                return BadRequest("User not found.");
            }

            await _userStatusService.UpdateUserOnlineStatus(user.Id, true);

            await _hubContext.Clients.All.SendAsync("UserStatusUpdated", new
            {
                Username = user.Username,
                IsOnline = true,
                LastSeen = DateTime.UtcNow
            });

            if (user.Roles != "Admin")
            {
                await _hubContext.Clients.Group("Admins").SendAsync("ReceiveLoginNotification", user.Username);
            }

            return Ok(result);
        }

        [HttpPost("Logout/{userId}")]
        public async Task<ActionResult> Logout(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (userId == Guid.Empty)
            {
                return BadRequest(new { message = "Invalid user ID." });
            }

            var result = await _authService.Logout(userId);
            _connectionManager.RemoveConnection(user.Username);
            if (result == "Logged out successfully." && _hubContext != null)
            {
                await _hubContext.Clients.Group("Admins").SendAsync("ReceiveLogoutNotification", $"{user.Username} has logged out.");
            }
            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken(RefreshTokenRequestDto request)
        {
            var result = await _authService.RefreshTokenAsync(request);

            if (result is null || result.AccessToken is null || result.RefreshToken is null) {
                return Unauthorized("Invalid refresh token");
            }

            return Ok(result);
        }

        [Authorize]
        [HttpGet]
        public IActionResult AuthenticatedOnlyEndPoint()
        {
            return Ok("You are authenticated");
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnlyEndPoint()
        {
            return Ok("You are an admin!");
        }

    }
}
