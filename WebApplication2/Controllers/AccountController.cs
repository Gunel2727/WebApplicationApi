using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using WebApplication2.Dtos;
using WebApplication2.Dtos.UserDtos;
using WebApplication2.Models;
using WebApplication2.Services;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(
        IValidator<RegisterDto> validator,
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration config,
        JwtService jwtService,
        IMapper mapper) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var validationResult = validator.Validate(registerDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }
            var user = await userManager.FindByNameAsync(registerDto.UserName);
            if (user is not null)
            {
                return BadRequest("UserName already exists");
            }
            user = mapper.Map<AppUser>(registerDto);
            var result = await userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            await userManager.AddToRoleAsync(user, "Member");
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var confirmLink = Url.Action(
                "ConfirmEmail",
                "Account",
                new { userId = user.Id, token = encodedToken },
                Request.Scheme
            );

            
            return Ok(new
            {
                message = "user registered successfully",
                confirmLink
            });

            
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await userManager.FindByNameAsync(loginDto.UserName);
            if (user is null)
            {
                return BadRequest("Invalid username or password");
            }
            var result = await userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!result)
            {
                return BadRequest("Invalid username or password");
            }
            if (!user.EmailConfirmed)
                return BadRequest("Please confirm your email first");
            var roles = await userManager.GetRolesAsync(user);
            var refreshToken = jwtService.GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await userManager.UpdateAsync(user);

            return Ok(new
            {
                accessToken = jwtService.GenerateToken(user, roles),
                refreshToken = refreshToken
            });
        }

        [HttpGet("profile")]
        [Authorize]
        public IActionResult Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.Identity?.Name;
            var fullName = User.FindFirstValue("FullName");
            var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray();
            return Ok(new
            {
                userId,
                userName,
                fullName,
                roles
            });
        }




        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
                return NotFound("User not found");
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token)); 
            var result = await userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
                return BadRequest("Invalid token");

            return Ok("Email confirmed successfully");
        }



        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email);

            if (user is null)
                return Ok("If email exists, reset link will be sent");

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(
            Encoding.UTF8.GetBytes(token));

            var resetLink = Url.Action(
                "ResetPassword",
                "Account",
                new { email = dto.Email, token = encodedToken },
                Request.Scheme
            );

            return Ok(new
            {
                resetLink
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email);

            if (user is null)
                return NotFound("User not found");
            var decodedToken = Encoding.UTF8.GetString(
                     WebEncoders.Base64UrlDecode(dto.Token));

            // ✅ DOĞRU
            var result = await userManager.ResetPasswordAsync(
                user,
                decodedToken,     // ← decoded token istifadə et
                dto.NewPassword
            );

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Password reset successfully");
        }


        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            var principal = jwtService.GetPrincipalFromExpiredToken(dto.AccessToken);
            if (principal is null)
                return BadRequest("Invalid access token");
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
                return BadRequest("Invalid access token");
            var user = await userManager.FindByIdAsync(userId);
            if (user is null ||
               user.RefreshToken != dto.RefreshToken ||
               user.RefreshTokenExpiry < DateTime.UtcNow)
                return BadRequest("Invalid or expired refresh token");
           
            var roles = await userManager.GetRolesAsync(user);

            var newAccessToken = jwtService.GenerateToken(user, roles); 
            var newRefreshToken = jwtService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await userManager.UpdateAsync(user);

            return Ok(new
            {
                accessToken = newAccessToken,
                refreshToken = newRefreshToken
            });
        }

        //[HttpGet("create-role")]
        //public async Task<IActionResult> CreateRole()
        //{
        //    await roleManager.CreateAsync(new IdentityRole("Member"));
        //    await roleManager.CreateAsync(new IdentityRole("Admin"));

        //    return Ok();
        //}
    }
}
