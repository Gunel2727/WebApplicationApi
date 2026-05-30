using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using System.Text;
using WebApplication2.Dtos;
using WebApplication2.Dtos.UserDtos;
using WebApplication2.Models;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class AccountService(
    UserManager<AppUser> userManager,
    JwtService jwtService,
    IMapper mapper) : IAccountService
    {
        public async Task<(bool Success, string? Error, string? ConfirmLink)> RegisterAsync(
            RegisterDto dto, Func<string, string, string?> urlGenerator)
        {
            var existingUser = await userManager.FindByNameAsync(dto.UserName);
            if (existingUser is not null)
                return (false, "UserName already exists", null);

            var user = mapper.Map<AppUser>(dto);
            var result = await userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return (false, result.Errors.First().Description, null);

            await userManager.AddToRoleAsync(user, "Member");
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var confirmLink = urlGenerator(user.Id, encodedToken);

            return (true, null, confirmLink);
        }

        public async Task<(bool Success, string? Error, string? AccessToken, string? RefreshToken)> LoginAsync(LoginDto dto)
        {
            var user = await userManager.FindByNameAsync(dto.UserName);
            if (user is null)
                return (false, "Invalid username or password", null, null);

            var result = await userManager.CheckPasswordAsync(user, dto.Password);
            if (!result)
                return (false, "Invalid username or password", null, null);

            if (!user.EmailConfirmed)
                return (false, "Please confirm your email first", null, null);

            var roles = await userManager.GetRolesAsync(user);
            var refreshToken = jwtService.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await userManager.UpdateAsync(user);

            return (true, null, jwtService.GenerateToken(user, roles), refreshToken);
        }

        public async Task<(bool Success, string? Error)> ConfirmEmailAsync(string userId, string token)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
                return (false, "User not found");

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await userManager.ConfirmEmailAsync(user, decodedToken);
            if (!result.Succeeded)
                return (false, "Invalid token");

            return (true, null);
        }

        public async Task<(bool Success, string? ResetLink)> ForgotPasswordAsync(
            string email, Func<string, string, string?> urlGenerator)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
                return (true, null); 

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var resetLink = urlGenerator(email, encodedToken);

            return (true, resetLink);
        }

        public async Task<(bool Success, string? Error)> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user is null)
                return (false, "User not found");

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));
            var result = await userManager.ResetPasswordAsync(user, decodedToken, dto.NewPassword);
            if (!result.Succeeded)
                return (false, result.Errors.First().Description);

            return (true, null);
        }

        public async Task<(bool Success, string? Error, string? AccessToken, string? RefreshToken)> RefreshTokenAsync(RefreshTokenDto dto)
        {
            var principal = jwtService.GetPrincipalFromExpiredToken(dto.AccessToken);
            if (principal is null)
                return (false, "Invalid access token", null, null);

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
                return (false, "Invalid access token", null, null);

            var user = await userManager.FindByIdAsync(userId);
            if (user is null ||
                user.RefreshToken != dto.RefreshToken ||
                user.RefreshTokenExpiry < DateTime.UtcNow)
                return (false, "Invalid or expired refresh token", null, null);

            var roles = await userManager.GetRolesAsync(user);
            var newAccessToken = jwtService.GenerateToken(user, roles);
            var newRefreshToken = jwtService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await userManager.UpdateAsync(user);

            return (true, null, newAccessToken, newRefreshToken);
        }
    }
}
