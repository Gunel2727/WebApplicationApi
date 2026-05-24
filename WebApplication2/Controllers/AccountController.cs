using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
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

            var confirmLink = Url.Action(
                "ConfirmEmail",
                "Account",
                new { userId = user.Id, token = token },
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

            var roles = await userManager.GetRolesAsync(user);


            return Ok(new
            {
                token = jwtService.GenerateToken(user, roles, config)
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

            var result = await userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
                return BadRequest("Invalid token");

            return Ok("Email confirmed successfully");
        }



        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user is null)
                return Ok("If email exists, reset link will be sent");

            var token = await userManager.GeneratePasswordResetTokenAsync(user);

            var resetLink = Url.Action(
                "ResetPassword",
                "Account",
                new { email = email, token = token },
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

            var result = await userManager.ResetPasswordAsync(
                user,
                dto.Token,
                dto.NewPassword
            );

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Password reset successfully");
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
