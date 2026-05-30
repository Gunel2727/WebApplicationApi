using WebApplication2.Dtos;
using WebApplication2.Dtos.UserDtos;

namespace WebApplication2.Services.Interfaces
{
    public interface IAccountService
    {
        Task<(bool Success, string? Error, string? ConfirmLink)> RegisterAsync(
            RegisterDto dto, Func<string, string, string?> urlGenerator);
        Task<(bool Success, string? Error, string? AccessToken, string? RefreshToken)> LoginAsync(LoginDto dto);
        Task<(bool Success, string? Error)> ConfirmEmailAsync(string userId, string token);
        Task<(bool Success, string? ResetLink)> ForgotPasswordAsync(string email, Func<string, string, string?> urlGenerator);
        Task<(bool Success, string? Error)> ResetPasswordAsync(ResetPasswordDto dto);
        Task<(bool Success, string? Error, string? AccessToken, string? RefreshToken)> RefreshTokenAsync(RefreshTokenDto dto);
    }
}
