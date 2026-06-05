using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApplication2.Controllers;
using WebApplication2.Dtos;
using WebApplication2.Dtos.UserDtos;
using WebApplication2.Services.Interfaces;

namespace TestWebProject
{
    public class AccountControllerTests
    {
        private readonly Mock<IAccountService> _mockService;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            _mockService = new Mock<IAccountService>();
            _controller = new AccountController(_mockService.Object);
        }

        // ================= REGISTER =================

        [Fact]
        public async Task Register_ReturnsOk_WhenSuccess()
        {
            var dto = new RegisterDto
            {
                UserName = "newuser",
                Password = "Test123!"
            };

            _mockService.Setup(x => x.RegisterAsync(
                    It.IsAny<RegisterDto>(),
                    It.IsAny<Func<string, string, string?>>()))
                .ReturnsAsync((true, null, "http://confirm-link"));

            var result = await _controller.Register(dto);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenUserNameExists()
        {
            var dto = new RegisterDto
            {
                UserName = "existinguser",
                Password = "Test123!"
            };

            _mockService.Setup(x => x.RegisterAsync(
                    It.IsAny<RegisterDto>(),
                    It.IsAny<Func<string, string, string?>>()))
                .ReturnsAsync((false, "UserName already exists", null));

            var result = await _controller.Register(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // ================= LOGIN =================

        [Fact]
        public async Task Login_ReturnsOk_WhenCredentialsValid()
        {
            var dto = new LoginDto
            {
                UserName = "testuser",
                Password = "Test123!"
            };

            _mockService.Setup(x => x.LoginAsync(It.IsAny<LoginDto>()))
                .ReturnsAsync((true, null, "accessToken", "refreshToken"));

            var result = await _controller.Login(dto);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Login_ReturnsBadRequest_WhenCredentialsInvalid()
        {
            var dto = new LoginDto
            {
                UserName = "testuser",
                Password = "WrongPass"
            };

            _mockService.Setup(x => x.LoginAsync(It.IsAny<LoginDto>()))
                .ReturnsAsync((false, "Invalid username or password", null, null));

            var result = await _controller.Login(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_ReturnsBadRequest_WhenEmailNotConfirmed()
        {
            var dto = new LoginDto
            {
                UserName = "testuser",
                Password = "Test123!"
            };

            _mockService.Setup(x => x.LoginAsync(It.IsAny<LoginDto>()))
                .ReturnsAsync((false, "Please confirm your email first", null, null));

            var result = await _controller.Login(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // ================= CONFIRM EMAIL =================

        [Fact]
        public async Task ConfirmEmail_ReturnsOk_WhenSuccess()
        {
            _mockService.Setup(x =>
                    x.ConfirmEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((true, null));

            var result = await _controller.ConfirmEmail("userId", "token");

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ConfirmEmail_ReturnsNotFound_WhenUserNotFound()
        {
            _mockService.Setup(x =>
                    x.ConfirmEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((false, "User not found"));

            var result = await _controller.ConfirmEmail("userId", "token");

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task ConfirmEmail_ReturnsBadRequest_WhenTokenInvalid()
        {
            _mockService.Setup(x =>
                    x.ConfirmEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((false, "Invalid token"));

            var result = await _controller.ConfirmEmail("userId", "token");

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // ================= FORGOT PASSWORD =================

        [Fact]
        public async Task ForgotPassword_ReturnsOk_WhenEmailNotFound()
        {
            _mockService.Setup(x =>
                    x.ForgotPasswordAsync(
                        It.IsAny<string>(),
                        It.IsAny<Func<string, string, string?>>()))
                .ReturnsAsync((true, (string?)null));

            var result = await _controller.ForgotPassword(
                new ForgotPasswordDto
                {
                    Email = "missing@gmail.com"
                });

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ForgotPassword_ReturnsOk_WithResetLink()
        {
            _mockService.Setup(x =>
                    x.ForgotPasswordAsync(
                        It.IsAny<string>(),
                        It.IsAny<Func<string, string, string?>>()))
                .ReturnsAsync((true, "http://reset-link"));

            var result = await _controller.ForgotPassword(
                new ForgotPasswordDto
                {
                    Email = "test@gmail.com"
                });

            Assert.IsType<OkObjectResult>(result);
        }

        // ================= RESET PASSWORD =================

        [Fact]
        public async Task ResetPassword_ReturnsOk_WhenSuccess()
        {
            var dto = new ResetPasswordDto
            {
                Email = "test@gmail.com",
                Token = "token",
                NewPassword = "New123!"
            };

            _mockService.Setup(x =>
                    x.ResetPasswordAsync(It.IsAny<ResetPasswordDto>()))
                .ReturnsAsync((true, null));

            var result = await _controller.ResetPassword(dto);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ResetPassword_ReturnsNotFound_WhenUserNotFound()
        {
            var dto = new ResetPasswordDto
            {
                Email = "wrong@gmail.com",
                Token = "token",
                NewPassword = "New123!"
            };

            _mockService.Setup(x =>
                    x.ResetPasswordAsync(It.IsAny<ResetPasswordDto>()))
                .ReturnsAsync((false, "User not found"));

            var result = await _controller.ResetPassword(dto);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task ResetPassword_ReturnsBadRequest_WhenTokenInvalid()
        {
            var dto = new ResetPasswordDto
            {
                Email = "test@gmail.com",
                Token = "badToken",
                NewPassword = "New123!"
            };

            _mockService.Setup(x =>
                    x.ResetPasswordAsync(It.IsAny<ResetPasswordDto>()))
                .ReturnsAsync((false, "Invalid token"));

            var result = await _controller.ResetPassword(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // ================= REFRESH TOKEN =================

        [Fact]
        public async Task RefreshToken_ReturnsOk_WhenSuccess()
        {
            var dto = new RefreshTokenDto
            {
                AccessToken = "oldAccess",
                RefreshToken = "oldRefresh"
            };

            _mockService.Setup(x =>
                    x.RefreshTokenAsync(It.IsAny<RefreshTokenDto>()))
                .ReturnsAsync((true, null, "newAccess", "newRefresh"));

            var result = await _controller.RefreshToken(dto);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task RefreshToken_ReturnsBadRequest_WhenTokenInvalid()
        {
            var dto = new RefreshTokenDto
            {
                AccessToken = "badAccess",
                RefreshToken = "badRefresh"
            };

            _mockService.Setup(x =>
                    x.RefreshTokenAsync(It.IsAny<RefreshTokenDto>()))
                .ReturnsAsync((false, "Invalid or expired refresh token", null, null));

            var result = await _controller.RefreshToken(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}