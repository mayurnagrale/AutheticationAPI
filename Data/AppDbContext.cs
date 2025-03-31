using AutheticationAPI.Entities;
using AutheticationAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AutheticationAPI.Data
{
    public class AppDbContext : DbContext  // Keep the class name as AppDbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserStatus> UserStatus { get; set; }
        public DbSet<Message> Messages { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) // Correct constructor
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Configure the one-to-one relationship between User and UserStatus ---
            modelBuilder.Entity<User>()
                .HasOne(u => u.UserStatus)
                .WithOne(us => us.User)
                .HasForeignKey<UserStatus>(us => us.UserId)
                .IsRequired();

            // --- Configure relationships for Message (optional, but good practice) ---
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany() // Assuming User doesn't have a direct Messages collection
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict); // Or .NoAction, or .ClientSetNull

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Recipient)
                .WithMany()
                .HasForeignKey(m => m.RecipientId)
                .OnDelete(DeleteBehavior.Restrict); // Or .NoAction, or .ClientSetNull

            // --- Configure User table name (OPTIONAL - if you want to rename it) ---
            modelBuilder.Entity<User>().ToTable("Users"); // Example: Rename to "AppUsers"
        }
    }

}
