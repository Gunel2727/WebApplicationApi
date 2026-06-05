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
using WebApplication2.Helpers;
using WebApplication2.Models;
using WebApplication2.Services;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(IAccountService accountService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var (success, error, confirmLink) = await accountService.RegisterAsync(dto,
                (userId, token) => Url.Action("ConfirmEmail", "Account",
                    new { userId, token }, Request.Scheme));

            if (!success)
                return BadRequest(ResponseModelHelper.CreateBadRequestResponse<object>(error));

            return Ok(ResponseModelHelper.CreateSuccessResponse(new
            {
                message = "user registered successfully",
                confirmLink
            }));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var (success, error, accessToken, refreshToken) = await accountService.LoginAsync(dto);
            if (!success)
                return BadRequest(ResponseModelHelper.CreateBadRequestResponse<object>(error));

            return Ok(ResponseModelHelper.CreateSuccessResponse(new
            {
                accessToken,
                refreshToken
            }));
        }

        [HttpGet("profile")]
        [Authorize]
        public IActionResult Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.Identity?.Name;
            var fullName = User.FindFirstValue("FullName");
            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value).ToArray();
            return Ok(ResponseModelHelper.CreateSuccessResponse(new
            {
                userId,
                userName,
                fullName,
                roles
            }));
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var (success, error) = await accountService.ConfirmEmailAsync(userId, token);
            if (!success)
                return error == "User not found"
                    ? NotFound(ResponseModelHelper.CreateNotFoundResponse<object>(error))
                    : BadRequest(ResponseModelHelper.CreateBadRequestResponse<object>(error));

            return Ok(ResponseModelHelper.CreateSuccessResponse("Email confirmed successfully"));
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var (_, resetLink) = await accountService.ForgotPasswordAsync(dto.Email,
                (email, token) => Url.Action("ResetPassword", "Account",
                    new { email, token }, Request.Scheme));
            if (resetLink is null)
                return Ok(ResponseModelHelper.CreateSuccessResponse("If email exists, reset link will be sent"));

            return Ok(ResponseModelHelper.CreateSuccessResponse(new { resetLink }));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            var (success, error) = await accountService.ResetPasswordAsync(dto);
            if (!success)
                return error == "User not found"
                    ? NotFound(ResponseModelHelper.CreateNotFoundResponse<object>(error))
                    : BadRequest(ResponseModelHelper.CreateBadRequestResponse<object>(error));

            return Ok(ResponseModelHelper.CreateSuccessResponse("Password reset successfully"));
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            var (success, error, accessToken, refreshToken) = await accountService.RefreshTokenAsync(dto);
            if (!success)
                return BadRequest(ResponseModelHelper.CreateBadRequestResponse<object>(error));

            return Ok(ResponseModelHelper.CreateSuccessResponse(new
            {
                accessToken,
                refreshToken
            }));
        }
    }

    //[HttpGet("create-role")]
    //public async Task<IActionResult> CreateRole()
    //{
    //    await roleManager.CreateAsync(new IdentityRole("Member"));
    //    await roleManager.CreateAsync(new IdentityRole("Admin"));

    //    return Ok();
    //}
}
