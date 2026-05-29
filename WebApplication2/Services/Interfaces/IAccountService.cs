using WebApplication2.Dtos.UserDtos;

namespace WebApplication2.Services.Interfaces
{
    public interface IAccountService
    {
        Task<(bool Success, string? Error, string? ConfirmLink)> RegisterAsync(RegisterDto dto, Func<string, string, string?> urlGenerator);
        Task<(bool Success, string? Error, string? AccessToken, string? RefreshToken)> LoginAsync(LoginDto dto);
    }
}
