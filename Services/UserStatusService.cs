using AutheticationAPI.Data;
using AutheticationAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AutheticationAPI.Services
{
    public interface IUserStatusService // Renamed interface
    {
        Task UpdateUserOnlineStatus(Guid userId, bool isOnline);
        Task<Guid?> GetUserIdByUsername(string username);
    }
    public class UserStatusService : IUserStatusService
    {

            private readonly AppDbContext _context;

            public UserStatusService(AppDbContext context)
            {
                _context = context;
            }

            public async Task UpdateUserOnlineStatus(Guid userId, bool isOnline)
            {
                var userStatus = await _context.UserStatus.FirstOrDefaultAsync(up => up.UserId == userId);
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (userStatus != null)
                {
                    userStatus.IsOnline = isOnline;
                    userStatus.LastSeen = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                else if (user != null)
                {
                    _context.UserStatus.Add(new UserStatus { UserId = userId, IsOnline = isOnline, LastSeen = DateTime.UtcNow });
                    await _context.SaveChangesAsync();
                }
            }

            public async Task<Guid?> GetUserIdByUsername(string username)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                return user?.Id;
            }
        }
}
