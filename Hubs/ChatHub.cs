using AutheticationAPI.Data;
using AutheticationAPI.Models;
using AutheticationAPI.Services;
using AutheticationAPI.Shared.Utility;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace AutheticationAPI.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ConnectionManager _connectionManager;
        private readonly AppDbContext _context;
        private readonly IUserStatusService _userStatusService;
        private readonly ILogger<ChatHub> _logger;


        public ChatHub(ConnectionManager connectionManager, IUserStatusService userStatusService, AppDbContext context, ILogger<ChatHub> logger)
        {
            _connectionManager = connectionManager;
            _userStatusService = userStatusService;
            _context = context; // Assign DbContext
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var username = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(username))
            {
                _connectionManager.AddConnection(username, Context.ConnectionId);
                Console.WriteLine($"User {username} connected to ChatHub with ConnectionId {Context.ConnectionId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var username = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(username))
            {
                _connectionManager.RemoveConnection(username);
                Console.WriteLine($"User {username} disconnected from ChatHub.");
            }

            await base.OnDisconnectedAsync(exception);
        }


        // --- Send message to a specific user ---
        public async Task SendMessageToUser(string recipient, string user, string message)
        {
            var connectionId = _connectionManager.GetConnection(recipient);
            var senderUserId = await _userStatusService.GetUserIdByUsername(user);
            var recipientUserId = await _userStatusService.GetUserIdByUsername(recipient);
            Console.WriteLine($"Message Received from {user} to {recipient} : {message}");
            try
            {
                if (!string.IsNullOrEmpty(connectionId))
                {
                    var newMessage = new Message
                    {
                        SenderId = senderUserId.Value,
                        RecipientId = recipientUserId.Value,
                        Content = message,
                        Timestamp = DateTime.UtcNow, // Use UTC for consistency
                        IsRead = true
                    };

                    _context.Messages.Add(newMessage);
                    await _context.SaveChangesAsync();

                    if (!string.IsNullOrEmpty(connectionId))
                    {
                        await Clients.Client(connectionId).SendAsync("ReceiveMessage", user, message);
                    }
                }
                else
                {
                    // Handle offline user (e.g., store the message for later delivery)
                    var newMessage = new Message
                    {
                        SenderId = senderUserId.Value,
                        RecipientId = recipientUserId.Value,
                        Content = message,
                        Timestamp = DateTime.UtcNow, // Use UTC for consistency
                        IsRead = false
                    };

                    _context.Messages.Add(newMessage);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            
        }

    }

}
