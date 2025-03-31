using AutheticationAPI.Data;
using AutheticationAPI.Models;
using AutheticationAPI.Services;
using AutheticationAPI.Shared.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Claims;

namespace AutheticationAPI.Hubs
{
    public class NotificationHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> OnlineUsers = new();

        private readonly ConnectionManager _connectionManager;
        private readonly AppDbContext _context;
        private readonly IUserStatusService _userStatusService;
        private readonly ILogger<ChatHub> _logger;


        public NotificationHub(ConnectionManager connectionManager, IUserStatusService userStatusService, AppDbContext context, ILogger<ChatHub> logger)
        {
            _connectionManager = connectionManager;
            _userStatusService = userStatusService;
            _context = context; // Assign DbContext
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            string username = Context.User?.Identity?.Name;

            if (!string.IsNullOrEmpty(username))
            {
                if (!OnlineUsers.ContainsKey(Context.ConnectionId))
                {
                    OnlineUsers[Context.ConnectionId] = username;

                    // If the connected user is an admin, add them to the admin group.
                    if (Context.User.IsInRole("Admin"))
                    {
                        await JoinAdminsGroup();
                    }
                }
            }
            else
            {
                _logger.LogWarning("User connected without a valid username.");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (OnlineUsers.TryRemove(Context.ConnectionId, out string username))
            {
                //Only need to notify all in this case, for demo purpose.
                //await Clients.All.SendAsync("ReceiveLogoutNotification", username);
            }

            if (exception != null)
            {
                _logger.LogError(exception, $"Error disconnecting user: {username}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task UpdateUserStatus(string username, bool isOnline)
        {
            await Clients.All.SendAsync("ReceiveUserStatusUpdate", username, isOnline);
        }

        public async Task GetOnlineUsers()
        {
            var users = OnlineUsers.Values.Distinct().ToList();
            await Clients.Caller.SendAsync("ReceiveOnlineUsers", users);
        }

        [Authorize(Roles = "Admin")]
        public async Task JoinAdminsGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            _logger.LogInformation($"User {Context.User?.Identity?.Name} joined the Admins group.");
        }
        public async Task SendLoginNotification(string username)
        {
            await Clients.Group("Admins").SendAsync("ReceiveLoginNotification", username);
            Console.WriteLine($"Login notification sent for: {username}");
        }

    }
}
