
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Controllers;
using WebApplication2.Services;
using WebApplication2.Dtos;
using WebApplication2.Services.Interfaces;
using WebApplication2.Dtos.OrganizerDtos;
using WebApplication2.Dtos.EventDtos;
using Moq;

namespace TestWebProject
{
    public class OrganizerControllerTests
    {
        private readonly Mock<IOrganizerService> _mockService;
        private readonly OrganizerController _controller;

        public OrganizerControllerTests()
        {
            _mockService = new Mock<IOrganizerService>();
            _controller = new OrganizerController(_mockService.Object, null!);
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
            var data = Assert.IsType<List<OrganizerReturnDto>>(okResult.Value);
            Assert.Equal(2, data.Count);
        }

        
        [Fact]
        public async Task GetEventsByOrganizer_ReturnsNotFound_WhenOrganizerNotExists()
        {
            
            _mockService.Setup(s => s.GetEventsByOrganizerAsync(99))
                        .ReturnsAsync((List<EventReturnDto>)null);

           
            var result = await _controller.GetEventsByOrganizer(99);

            
            Assert.IsType<NotFoundObjectResult>(result);
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
            var data = Assert.IsType<List<EventReturnDto>>(okResult.Value);
            Assert.Single(data);
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
            var data = Assert.IsType<OrganizerReturnDto>(okResult.Value);
            Assert.Equal("New Organizer", data.Name);
        }
    }
}
