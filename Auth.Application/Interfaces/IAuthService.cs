using Auth.Application.DTOs;
using Microsoft.AspNetCore.Identity;

namespace Auth.Application.Interfaces
{
    public interface IAuthService
    {
        Task<IdentityResult> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto?> LoginAsync(LoginDto dto);
        Task<AuthResponseDto?> RefreshTokenAsync(RefreshTokenDto dto);
        Task<bool> LogoutAsync(string userId);
    }
}
