using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Controllers;
using WebApplication2.Dtos;
using WebApplication2.Dtos.TicketDtos;
using WebApplication2.Services.Interfaces;

public class TicketControllerTests
{
    private readonly Mock<ITicketService> _mockService;
    private readonly TicketController _controller;

    public TicketControllerTests()
    {
        _mockService = new Mock<ITicketService>();
        _controller = new TicketController(_mockService.Object);
    }

   
    [Fact]
    public async Task GetAll_ReturnsOk_WithTickets()
    {
        
        var fakeTickets = new List<TicketReturnDto>
        {
            new TicketReturnDto { Id = 1, Type = "VIP" },
            new TicketReturnDto { Id = 2, Type = "Standard" }
        };
        _mockService.Setup(s => s.GetAllTicketsAsync())
                    .ReturnsAsync(fakeTickets);

        
        var result = await _controller.GetAll();

       
        var okResult = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsType<List<TicketReturnDto>>(okResult.Value);
        Assert.Equal(2, data.Count);
    }

    
    [Fact]
    public async Task Create_ReturnsNotFound_WhenEventNotExists()
    {
        
        _mockService.Setup(s => s.CreateTicketAsync(It.IsAny<TicketCreateDto>()))
                    .ReturnsAsync((TicketReturnDto)null);

        
        var result = await _controller.Create(new TicketCreateDto());

        
        Assert.IsType<NotFoundObjectResult>(result);
    }

   
    [Fact]
    public async Task Create_ReturnsOk_WithCreatedTicket()
    {
        
        var dto = new TicketCreateDto { Type = "VIP", Price = 100, QuantityAvailable = 50 };
        var fakeTicket = new TicketReturnDto { Id = 1, Type = "VIP" };
        _mockService.Setup(s => s.CreateTicketAsync(dto))
                    .ReturnsAsync(fakeTicket);

       
        var result = await _controller.Create(dto);

        
        var okResult = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsType<TicketReturnDto>(okResult.Value);
        Assert.Equal("VIP", data.Type);
    }
}
