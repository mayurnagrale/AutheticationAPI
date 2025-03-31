using AutheticationAPI.Entities;

namespace AutheticationAPI.Models
{
    public class UserStatus
    {
        public int Id { get; set; }
        public Guid UserId { get; set; } // Changed to Guid
        public bool IsOnline { get; set; }
        public DateTime LastSeen { get; set; }
        public User User { get; internal set; }
    }
}
