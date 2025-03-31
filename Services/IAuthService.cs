using AutheticationAPI.Entities;
using AutheticationAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace AutheticationAPI.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserModel request);
        Task<TokenResponseDto?> LoginAsync(UserModel request);
        Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request);
        Task<string> Logout(Guid id);
    }
}
