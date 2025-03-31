using AutheticationAPI.Entities;

namespace AutheticationAPI.Models
{
    public class Message
    {
        public int Id { get; set; } // Primary Key
        public Guid SenderId { get; set; }     // Foreign Key to Users table
        public Guid RecipientId { get; set; }  // Foreign Key to Users table
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; } = false; // Optional: Track if the message has been read

        // Navigation properties (optional, but very useful)
        public User Sender { get; set; }
        public User Recipient { get; set; }
    }
}
