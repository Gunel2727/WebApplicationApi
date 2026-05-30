using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWebProject
{
    using Moq;
    using Microsoft.AspNetCore.Mvc;
    using WebApplication2.Controllers;
    using WebApplication2.Services.Interfaces;
    using WebApplication2.Dtos;
    using WebApplication2.Dtos.UserDtos;

    public class AccountControllerTests
    {
        private readonly Mock<IAccountService> _mockService;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            _mockService = new Mock<IAccountService>();
            _controller = new AccountController(_mockService.Object);
        }

        // Test 1: Register - uğurlu
        [Fact]
        public async Task Register_ReturnsOk_WhenSuccess()
        {
            var dto = new RegisterDto { UserName = "newuser", Password = "Test123!" };
            _mockService.Setup(s => s.RegisterAsync(dto, It.IsAny<Func<string, string, string?>>()))
                        .ReturnsAsync((true, null, "http://confirm-link"));

            var result = await _controller.Register(dto);

            Assert.IsType<OkObjectResult>(result);
        }

        // Test 2: Register - username mövcuddur
        [Fact]
        public async Task Register_ReturnsBadRequest_WhenUserNameExists()
        {
            var dto = new RegisterDto { UserName = "existinguser", Password = "Test123!" };
            _mockService.Setup(s => s.RegisterAsync(dto, It.IsAny<Func<string, string, string?>>()))
                        .ReturnsAsync((false, "UserName already exists", null));

            var result = await _controller.Register(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // Test 3: Login - uğurlu
        [Fact]
        public async Task Login_ReturnsOk_WhenCredentialsValid()
        {
            var dto = new LoginDto { UserName = "testuser", Password = "Test123!" };
            _mockService.Setup(s => s.LoginAsync(dto))
                        .ReturnsAsync((true, null, "accessToken123", "refreshToken123"));

            var result = await _controller.Login(dto);

            Assert.IsType<OkObjectResult>(result);
        }

        // Test 4: Login - yanlış şifrə
        [Fact]
        public async Task Login_ReturnsBadRequest_WhenCredentialsInvalid()
        {
            var dto = new LoginDto { UserName = "testuser", Password = "WrongPass!" };
            _mockService.Setup(s => s.LoginAsync(dto))
                        .ReturnsAsync((false, "Invalid username or password", null, null));

            var result = await _controller.Login(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // Test 5: Login - email təsdiqlənməyib
        [Fact]
        public async Task Login_ReturnsBadRequest_WhenEmailNotConfirmed()
        {
            var dto = new LoginDto { UserName = "testuser", Password = "Test123!" };
            _mockService.Setup(s => s.LoginAsync(dto))
                        .ReturnsAsync((false, "Please confirm your email first", null, null));

            var result = await _controller.Login(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // Test 6: ConfirmEmail - uğurlu
        [Fact]
        public async Task ConfirmEmail_ReturnsOk_WhenSuccess()
        {
            _mockService.Setup(s => s.ConfirmEmailAsync("userId123", "token123"))
                        .ReturnsAsync((true, null));

            var result = await _controller.ConfirmEmail("userId123", "token123");

            Assert.IsType<OkObjectResult>(result);
        }

        // Test 7: ConfirmEmail - user tapılmır
        [Fact]
        public async Task ConfirmEmail_ReturnsNotFound_WhenUserNotFound()
        {
            _mockService.Setup(s => s.ConfirmEmailAsync("wrongId", "token123"))
                        .ReturnsAsync((false, "User not found"));

            var result = await _controller.ConfirmEmail("wrongId", "token123");

            Assert.IsType<NotFoundObjectResult>(result);
        }

        // Test 8: ConfirmEmail - invalid token
        [Fact]
        public async Task ConfirmEmail_ReturnsBadRequest_WhenTokenInvalid()
        {
            _mockService.Setup(s => s.ConfirmEmailAsync("userId123", "badToken"))
                        .ReturnsAsync((false, "Invalid token"));

            var result = await _controller.ConfirmEmail("userId123", "badToken");

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // Test 9: ForgotPassword - email mövcud deyil
        [Fact]
        public async Task ForgotPassword_ReturnsOk_WhenEmailNotFound()
        {
            _mockService.Setup(s => s.ForgotPasswordAsync(
                            "notexist@gmail.com",
                            It.IsAny<Func<string, string, string?>>()))
                        .ReturnsAsync((true, (string?)null));

            var result = await _controller.ForgotPassword(new ForgotPasswordDto { Email = "notexist@gmail.com" });

            Assert.IsType<OkObjectResult>(result);
        }

        // Test 10: ForgotPassword - reset link qaytarır
        [Fact]
        public async Task ForgotPassword_ReturnsOk_WithResetLink()
        {
            _mockService.Setup(s => s.ForgotPasswordAsync(
                            "test@gmail.com",
                            It.IsAny<Func<string, string, string?>>()))
                        .ReturnsAsync((true, "http://reset-link"));

            var result = await _controller.ForgotPassword(new ForgotPasswordDto { Email = "test@gmail.com" });

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        // Test 11: ResetPassword - uğurlu
        [Fact]
        public async Task ResetPassword_ReturnsOk_WhenSuccess()
        {
            var dto = new ResetPasswordDto { Email = "test@gmail.com", Token = "token", NewPassword = "New123!" };
            _mockService.Setup(s => s.ResetPasswordAsync(dto))
                        .ReturnsAsync((true, null));

            var result = await _controller.ResetPassword(dto);

            Assert.IsType<OkObjectResult>(result);
        }

        // Test 12: ResetPassword - user tapılmır
        [Fact]
        public async Task ResetPassword_ReturnsNotFound_WhenUserNotFound()
        {
            var dto = new ResetPasswordDto { Email = "wrong@gmail.com", Token = "token", NewPassword = "New123!" };
            _mockService.Setup(s => s.ResetPasswordAsync(dto))
                        .ReturnsAsync((false, "User not found"));

            var result = await _controller.ResetPassword(dto);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        // Test 13: ResetPassword - invalid token
        [Fact]
        public async Task ResetPassword_ReturnsBadRequest_WhenTokenInvalid()
        {
            var dto = new ResetPasswordDto { Email = "test@gmail.com", Token = "badToken", NewPassword = "New123!" };
            _mockService.Setup(s => s.ResetPasswordAsync(dto))
                        .ReturnsAsync((false, "Invalid token"));

            var result = await _controller.ResetPassword(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // Test 14: RefreshToken - uğurlu
        [Fact]
        public async Task RefreshToken_ReturnsOk_WhenSuccess()
        {
            var dto = new RefreshTokenDto { AccessToken = "oldAccess", RefreshToken = "oldRefresh" };
            _mockService.Setup(s => s.RefreshTokenAsync(dto))
                        .ReturnsAsync((true, null, "newAccess", "newRefresh"));

            var result = await _controller.RefreshToken(dto);

            Assert.IsType<OkObjectResult>(result);
        }

        // Test 15: RefreshToken - invalid token
        [Fact]
        public async Task RefreshToken_ReturnsBadRequest_WhenTokenInvalid()
        {
            var dto = new RefreshTokenDto { AccessToken = "badToken", RefreshToken = "badRefresh" };
            _mockService.Setup(s => s.RefreshTokenAsync(dto))
                        .ReturnsAsync((false, "Invalid or expired refresh token", null, null));

            var result = await _controller.RefreshToken(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
