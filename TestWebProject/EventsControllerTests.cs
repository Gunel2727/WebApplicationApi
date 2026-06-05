using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Controllers;
using WebApplication2.Dtos;
using WebApplication2.Dtos.EventDtos;
using WebApplication2.Dtos.OrganizerDtos;
using WebApplication2.Dtos.TicketDtos;
using WebApplication2.Models;
using WebApplication2.Services.Interfaces;

namespace TestWebProject
{
    public class EventsControllerTests
    {
        private readonly Mock<IEventService> _mockService;
        private readonly EventsController _controller;

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
        public EventsControllerTests()
        {
            _mockService = new Mock<IEventService>();
            _controller = new EventsController(_mockService.Object);
                SetupHttpContext();
        }

        [Fact]
        public async Task GetAllEvents_ReturnsOk_WithEvents()
        {
            var fakeEvents = new List<EventReturnDto>
    {
        new EventReturnDto { Id = 1, Title = "Event 1" },
        new EventReturnDto { Id = 2, Title = "Event 2" }
    };

            _mockService.Setup(s => s.GetAllEventsAsync())
                .ReturnsAsync(fakeEvents);

            var result = await _controller.GetAllEvents();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var response =
                Assert.IsType<ResponseModel<List<EventReturnDto>>>(okResult.Value);

            Assert.True(response.Success);
            Assert.Equal(2, response.Data.Count);
        }

        [Fact]
        public async Task GetTicketsByEvent_ReturnsNotFound_WhenEventNotExists()
        {
            _mockService.Setup(s => s.GetTicketsByEventAsync(99))
                .ReturnsAsync((List<TicketReturnDto>?)null);

            var result = await _controller.GetTicketsByEvent(99);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);

            var response =
                Assert.IsType<ResponseModel<object>>(notFoundResult.Value);

            Assert.False(response.Success);
        }


        [Fact]
        public async Task GetTicketsByEvent_ReturnsOk_WithTickets()
        {
            var fakeTickets = new List<TicketReturnDto>
    {
        new TicketReturnDto { Id = 1, Type = "VIP" }
    };

            _mockService.Setup(s => s.GetTicketsByEventAsync(1))
                .ReturnsAsync(fakeTickets);

            var result = await _controller.GetTicketsByEvent(1);

            var okResult = Assert.IsType<OkObjectResult>(result);

            var response =
                Assert.IsType<ResponseModel<List<TicketReturnDto>>>(okResult.Value);

            Assert.True(response.Success);
            Assert.Single(response.Data);
        }


        [Fact]
        public async Task GetOrganizerByEvent_ReturnsNotFound_WhenNotExists()
        {
            _mockService.Setup(s => s.GetOrganizerByEventAsync(99))
                .ReturnsAsync((OrganizerReturnDto?)null);

            var result = await _controller.GetOrganizerByEvent(99);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);

            var response =
                Assert.IsType<ResponseModel<object>>(notFoundResult.Value);

            Assert.False(response.Success);
        }

        [Fact]
        public async Task GetOrganizerByEvent_ReturnsOk_WithOrganizer()
        {
            var fakeOrganizer = new OrganizerReturnDto
            {
                Id = 1,
                Name = "Organizer 1"
            };

            _mockService.Setup(s => s.GetOrganizerByEventAsync(1))
                .ReturnsAsync(fakeOrganizer);

            var result = await _controller.GetOrganizerByEvent(1);

            var okResult = Assert.IsType<OkObjectResult>(result);

            var response =
                Assert.IsType<ResponseModel<OrganizerReturnDto>>(okResult.Value);

            Assert.True(response.Success);
            Assert.Equal("Organizer 1", response.Data.Name);
        }


        [Fact]
        public async Task CreateTicketForEvent_ReturnsNotFound_WhenEventNotExists()
        {
            _mockService.Setup(s => s.CreateTicketForEventAsync(
                99,
                It.IsAny<TicketCreateDto>()))
                .ReturnsAsync((TicketReturnDto?)null);

            var result = await _controller.CreateTicketForEvent(
                99,
                new TicketCreateDto());

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);

            var response =
                Assert.IsType<ResponseModel<object>>(notFoundResult.Value);

            Assert.False(response.Success);
        }

       

            [Fact]
            public async Task CreateTicketForEvent_ReturnsOk_WithTicket()
            {
                var dto = new TicketCreateDto
                {
                    Type = "VIP",
                    Price = 100,
                    QuantityAvailable = 50
                };

                var fakeTicket = new TicketReturnDto
                {
                    Id = 1,
                    Type = "VIP"
                };

                _mockService.Setup(s => s.CreateTicketForEventAsync(1, dto))
                    .ReturnsAsync(fakeTicket);

                var result = await _controller.CreateTicketForEvent(1, dto);

                var okResult = Assert.IsType<OkObjectResult>(result);

                var response =
                    Assert.IsType<ResponseModel<TicketReturnDto>>(okResult.Value);

                Assert.True(response.Success);
                Assert.Equal("VIP", response.Data.Type);
            }
        [Fact]
        public async Task UploadBanner_ReturnsNotFound_WhenEventNotExists()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.ContentType).Returns("image/jpeg");
            mockFile.Setup(f => f.FileName).Returns("banner.jpg");

            _mockService.Setup(s => s.UploadBannerAsync(
                    99,
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync((UploadBannerReturnDto)null);

            var result = await _controller.UploadBanner(99, mockFile.Object);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<ResponseModel<object>>(notFoundResult.Value);
            Assert.False(response.Success);
        }
        [Fact]
        public async Task UploadBanner_ReturnsOk_WhenSuccess()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.ContentType).Returns("image/jpeg");
            mockFile.Setup(f => f.FileName).Returns("banner.jpg");

            var fakeResult = new UploadBannerReturnDto
            {
                FileName = "banner.jpg",
                Path = "/images/banner.jpg",
                Url = "https://localhost:7268/images/banner.jpg"
            };

            _mockService.Setup(s => s.UploadBannerAsync(
                    1,
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(fakeResult);

            var result = await _controller.UploadBanner(1, mockFile.Object);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel<UploadBannerReturnDto>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("banner.jpg", response.Data.FileName);
        }
        [Fact]
        public async Task UploadBanner_ReturnsBadRequest_WhenFileIsEmpty()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0);

            var result = await _controller.UploadBanner(1, mockFile.Object);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel<object>>(badRequestResult.Value);
            Assert.False(response.Success);
        }
        [Fact]
        public async Task UploadBanner_ReturnsBadRequest_WhenFileIsNotImage()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.ContentType).Returns("application/pdf");
            mockFile.Setup(f => f.FileName).Returns("test.pdf");

            var result = await _controller.UploadBanner(1, mockFile.Object);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ResponseModel<object>>(badRequestResult.Value);
            Assert.False(response.Success);
        }

    }




    
}
