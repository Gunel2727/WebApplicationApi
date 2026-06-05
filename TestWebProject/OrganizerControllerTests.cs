
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApplication2.Controllers;
using WebApplication2.Dtos;
using WebApplication2.Dtos.EventDtos;
using WebApplication2.Dtos.OrganizerDtos;
using WebApplication2.Models;
using WebApplication2.Services;
using WebApplication2.Services.Interfaces;

namespace TestWebProject
{
    public class OrganizerControllerTests
    {
        private readonly Mock<IOrganizerService> _mockService;
        private readonly OrganizerController _controller;
        private void SetupHttpContext()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = "https";
            httpContext.Request.Host = new HostString("localhost", 7268);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        public OrganizerControllerTests()
        {
            _mockService = new Mock<IOrganizerService>();
            _controller = new OrganizerController(_mockService.Object);
            SetupHttpContext();
        }
       

        [Fact]
        public async Task GetAllOrganizers_ReturnsOk_WithOrganizers()
        {
            
            var fakeOrganizers = new List<OrganizerReturnDto>
        {
            new OrganizerReturnDto { Id = 1, Name = "Organizer 1" },
            new OrganizerReturnDto { Id = 2, Name = "Organizer 2" }
        };
            _mockService.Setup(s => s.GetAllOrganizersAsync())
                        .ReturnsAsync(fakeOrganizers);

            
            var result = await _controller.GetAllOrganizers();

           
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel<List<OrganizerReturnDto>>>(okResult.Value);

            Assert.True(response.Success);
            Assert.Equal(2, response.Data.Count);
        }

        
        [Fact]
        public async Task GetEventsByOrganizer_ReturnsNotFound_WhenOrganizerNotExists()
        {
            
            _mockService.Setup(s => s.GetEventsByOrganizerAsync(99))
                        .ReturnsAsync((List<EventReturnDto>)null);

           
            var result = await _controller.GetEventsByOrganizer(99);


            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<ResponseModel<object>>(notFoundResult.Value);
            Assert.False(response.Success);
        }

     
        [Fact]
        public async Task GetEventsByOrganizer_ReturnsOk_WithEvents()
        {
          
            var fakeEvents = new List<EventReturnDto>
        {
            new EventReturnDto { Id = 1, Title = "Event 1" }
        };
            _mockService.Setup(s => s.GetEventsByOrganizerAsync(1))
                        .ReturnsAsync(fakeEvents);

            
            var result = await _controller.GetEventsByOrganizer(1);


            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel<List<EventReturnDto>>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Single(response.Data);
        }

       
        [Fact]
        public async Task Create_ReturnsOk_WithCreatedOrganizer()
        {
           
            var dto = new OrganizerCreateDto { Name = "New Organizer" };
            var fakeOrganizer = new OrganizerReturnDto { Id = 1, Name = "New Organizer" };
            _mockService.Setup(s => s.CreateOrganizerAsync(dto))
                        .ReturnsAsync(fakeOrganizer);

            
            var result = await _controller.Create(dto);


            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel<OrganizerReturnDto>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("New Organizer", response.Data.Name);
        }
        [Fact]
        public async Task UploadLogo_ReturnsNotFound_WhenOrganizerNotExists()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.ContentType).Returns("image/jpeg");
            mockFile.Setup(f => f.FileName).Returns("test.jpg");

            _mockService.Setup(s => s.UploadLogoAsync(99, It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((UploadLogoReturnDto)null);

            var result = await _controller.UploadLogo(99, mockFile.Object);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<ResponseModel<object>>(notFoundResult.Value);
            Assert.False(response.Success);
        }

        [Fact]
        public async Task UploadLogo_ReturnsOk_WhenSuccess()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.ContentType).Returns("image/jpeg");
            mockFile.Setup(f => f.FileName).Returns("test.jpg");

            var fakeResult = new UploadLogoReturnDto
            {
                FileName = "test.jpg",
                Path = "/images/test.jpg",
                Url = "https://localhost:7268/images/test.jpg"
            };

            _mockService.Setup(s => s.UploadLogoAsync(1, It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(fakeResult);

            var result = await _controller.UploadLogo(1, mockFile.Object);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel<UploadLogoReturnDto>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("test.jpg", response.Data.FileName);
        }

        [Fact]
        public async Task UploadLogo_ReturnsBadRequest_WhenFileIsEmpty()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0);

            var result = await _controller.UploadLogo(1, mockFile.Object);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel<object>>(badRequestResult.Value);
            Assert.False(response.Success);
        }

        [Fact]
        public async Task UploadLogo_ReturnsBadRequest_WhenFileIsNotImage()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.ContentType).Returns("application/pdf");
            mockFile.Setup(f => f.FileName).Returns("test.pdf");

            var result = await _controller.UploadLogo(1, mockFile.Object);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel<object>>(badRequestResult.Value);
            Assert.False(response.Success);
        }
    }
}
