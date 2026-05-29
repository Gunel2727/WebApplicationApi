using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Controllers;
using WebApplication2.Dtos.EventDtos;
using WebApplication2.Dtos.OrganizerDtos;
using WebApplication2.Dtos.TicketDtos;
using WebApplication2.Services.Interfaces;

namespace TestWebProject
{
    public class EventsControllerTests
    {
        private readonly Mock<IEventService> _mockService;
        private readonly EventsController _controller;

        public EventsControllerTests()
        {
            _mockService = new Mock<IEventService>();
            _controller = new EventsController(_mockService.Object, null!);
        }

        [Fact]
        public async Task GetAllEvents_ReturnsOk_WithEvents()
        {
            // Arrange
            var fakeEvents = new List<EventReturnDto>
        {
            new EventReturnDto { Id = 1, Title = "Event 1" },
            new EventReturnDto { Id = 2, Title = "Event 2" }
        };
            _mockService.Setup(s => s.GetAllEventsAsync())
                        .ReturnsAsync(fakeEvents);

            
            var result = await _controller.GetAllEvents();

            
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<List<EventReturnDto>>(okResult.Value);
            Assert.Equal(2, data.Count);
        }

       
        [Fact]
        public async Task GetTicketsByEvent_ReturnsNotFound_WhenEventNotExists()
        {
           
            _mockService.Setup(s => s.GetTicketsByEventAsync(99))
                        .ReturnsAsync((List<TicketReturnDto>)null);

           
            var result = await _controller.GetTicketsByEvent(99);

            
            Assert.IsType<NotFoundObjectResult>(result);
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
            var data = Assert.IsType<List<TicketReturnDto>>(okResult.Value);
            Assert.Single(data);
        }

        
        [Fact]
        public async Task GetOrganizerByEvent_ReturnsNotFound_WhenNotExists()
        {
            
            _mockService.Setup(s => s.GetOrganizerByEventAsync(99))
                        .ReturnsAsync((OrganizerReturnDto)null);

            
            var result = await _controller.GetOrganizerByEvent(99);

            
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetOrganizerByEvent_ReturnsOk_WithOrganizer()
        {
            
            var fakeOrganizer = new OrganizerReturnDto { Id = 1, Name = "Organizer 1" };
            _mockService.Setup(s => s.GetOrganizerByEventAsync(1))
                        .ReturnsAsync(fakeOrganizer);

           
            var result = await _controller.GetOrganizerByEvent(1);

           
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<OrganizerReturnDto>(okResult.Value);
            Assert.Equal("Organizer 1", data.Name);
        }

        
        [Fact]
        public async Task CreateTicketForEvent_ReturnsNotFound_WhenEventNotExists()
        {
            
            _mockService.Setup(s => s.CreateTicketForEventAsync(99, It.IsAny<TicketCreateDto>()))
                        .ReturnsAsync((TicketReturnDto)null);

           
            var result = await _controller.CreateTicketForEvent(99, new TicketCreateDto());

            Assert.IsType<NotFoundObjectResult>(result);
        }

    
        [Fact]
        public async Task CreateTicketForEvent_ReturnsOk_WithTicket()
        {
            
            var dto = new TicketCreateDto { Type = "VIP", Price = 100, QuantityAvailable = 50 };
            var fakeTicket = new TicketReturnDto { Id = 1, Type = "VIP" };
            _mockService.Setup(s => s.CreateTicketForEventAsync(1, dto))
                        .ReturnsAsync(fakeTicket);

            var result = await _controller.CreateTicketForEvent(1, dto);

            
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<TicketReturnDto>(okResult.Value);
            Assert.Equal("VIP", data.Type);
        }



    }
}
