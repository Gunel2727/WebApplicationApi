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
using WebApplication2.Models;
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
        // Arrange
        var fakeTickets = new List<TicketReturnDto>
    {
        new TicketReturnDto { Id = 1, Type = "VIP" },
        new TicketReturnDto { Id = 2, Type = "Standard" }
    };

        _mockService.Setup(s => s.GetAllTicketsAsync())
                    .ReturnsAsync(fakeTickets);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);

        var response =
            Assert.IsType<ResponseModel<List<TicketReturnDto>>>(okResult.Value);

        Assert.True(response.Success);
        Assert.Equal(2, response.Data.Count);
    }

    [Fact]
    public async Task Create_ReturnsNotFound_WhenEventNotExists()
    {
        // Arrange
        _mockService.Setup(s => s.CreateTicketAsync(It.IsAny<TicketCreateDto>()))
                    .ReturnsAsync((TicketReturnDto?)null);

        // Act
        var result = await _controller.Create(new TicketCreateDto());

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);

        var response =
            Assert.IsType<ResponseModel<Ticket>>(notFoundResult.Value);

        Assert.False(response.Success);
    }


    [Fact]
    public async Task Create_ReturnsOk_WithCreatedTicket()
    {
        // Arrange
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

        _mockService.Setup(s => s.CreateTicketAsync(dto))
                    .ReturnsAsync(fakeTicket);

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);

        var response =
            Assert.IsType<ResponseModel<TicketReturnDto>>(okResult.Value);

        Assert.True(response.Success);
        Assert.Equal("VIP", response.Data.Type);
    }
}
