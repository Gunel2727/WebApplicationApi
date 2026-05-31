using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Dtos.TicketDtos;
using WebApplication2.Helpers;
using WebApplication2.Models;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController(ITicketService ticketService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tickets = await ticketService.GetAllTicketsAsync();
            return Ok(ResponseModelHelper.CreateSuccessResponse(tickets));
        }

        [HttpPost]
        public async Task<IActionResult> Create(TicketCreateDto dto)
        {
            var ticket = await ticketService.CreateTicketAsync(dto);
            if (ticket is null)
                return NotFound(ResponseModelHelper.CreateNotFoundResponse<Ticket>("Event tapılmadı"));

            return Ok(ResponseModelHelper.CreateSuccessResponse(ticket));
        }
    
    }
}
