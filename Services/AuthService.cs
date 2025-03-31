using AutheticationAPI.Data;
using AutheticationAPI.Entities;
using AutheticationAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AutheticationAPI.Services
{
    public class AuthService(AppDbContext context, IConfiguration configuration, IUserStatusService userStatusService) : IAuthService
    {
        private readonly IUserStatusService _userStatusService = userStatusService;
        public async Task<TokenResponseDto?> LoginAsync(UserModel request)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user is null)
            {
                return null;
            }

            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.PasswordHash) == PasswordVerificationResult.Failed)
            {
                return null;
            }

            return await CreateTokenResponse(user);
        }

        private async Task<TokenResponseDto> CreateTokenResponse(User? user)
        {
            return new TokenResponseDto { AccessToken = CreateToken(user), RefreshToken = await GenerateAndSaveRefreshTokenAsync(user),UserId = user.Id };
        }

        public async Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var user = await ValidateRefreshTokenAysnc(request.UserId, request.RefreshToken);
            if (user is null)
            {
                return null;
            }

            return await CreateTokenResponse(user);
        }

        public async Task<User?> RegisterAsync(UserModel request)
        {
            if(await context.Users.AnyAsync(u=>u.Username == request.Username))
            {
                return null;
            }

            User user = new User();
            var HashPassword = new PasswordHasher<User>().HashPassword(user, request.PasswordHash);

            user.Username = request.Username;
            user.PasswordHash = HashPassword;

            user.Roles = "User";

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return user;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            var refreshtoken = GenerateRefreshToken();
            user.RefreshToken = refreshtoken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await context.SaveChangesAsync();
            return refreshtoken;
        }

        private async Task<User?> ValidateRefreshTokenAysnc(Guid userId,string refreshToken)
        {
            var user = await context.Users.FindAsync(userId);
            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiry > DateTime.UtcNow) {
                return null;
            }

            return user;
        }
        private string CreateToken(User user)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.Username), new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Role, user.Roles.ToString()) };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("Jwt:Secret")!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "me",
                audience: "me",
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> Logout(Guid id)
        {
            try
            {
                var user = await context.Users.FindAsync(id);  //Fetch using id
                if (user is null)
                {
                    return "User not found.";
                }
                await _userStatusService.UpdateUserOnlineStatus(id, false);
                return "Logged out successfully.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during logout: {ex.Message}");
                return "An error occurred during logout.";
            }
        }
    }
}
