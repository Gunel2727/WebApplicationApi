using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Dtos.EventDtos;
using WebApplication2.Dtos.OrganizerDtos;
using WebApplication2.Dtos.TicketDtos;
using WebApplication2.Extensions;
using WebApplication2.Helpers;
using WebApplication2.Models;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController(IEventService eventService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await eventService.GetAllEventsAsync();
            return Ok(ResponseModelHelper.CreateSuccessResponse(events));
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm]EventCreateDto dto)
        {
            var ev = await eventService.CreateEventAsync(dto);
            return Ok(ResponseModelHelper.CreateSuccessResponse(ev));
        }

        [HttpPost("{id}/banner")]
        public async Task<IActionResult> UploadBanner(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ResponseModelHelper.CreateBadRequestResponse<object>("File is required"));
            if (!file.IsImage())
                return BadRequest(ResponseModelHelper.CreateBadRequestResponse<object>("Only image files are allowed"));
            if (!file.IsValidSize(2))
                return BadRequest(ResponseModelHelper.CreateBadRequestResponse<object>("File size must be less than 2MB"));

            var result = await eventService.UploadBannerAsync(id, file, Request.Scheme, Request.Host.ToString());
            if (result is null)
                return NotFound(ResponseModelHelper.CreateNotFoundResponse<object>("Event not found"));

            return Ok(ResponseModelHelper.CreateSuccessResponse(result));
        }

        [HttpGet("{eventId}/tickets")]
        public async Task<IActionResult> GetTicketsByEvent(int eventId)
        {
            var tickets = await eventService.GetTicketsByEventAsync(eventId);
            if (tickets is null)
                return NotFound(ResponseModelHelper.CreateNotFoundResponse<object>("Event not found"));
            return Ok(ResponseModelHelper.CreateSuccessResponse(tickets));
        }

        [HttpGet("{eventId}/organizer")]
        public async Task<IActionResult> GetOrganizerByEvent(int eventId)
        {
            var organizer = await eventService.GetOrganizerByEventAsync(eventId);
            if (organizer is null)
                return NotFound(ResponseModelHelper.CreateNotFoundResponse<object>("Not found"));
            return Ok(ResponseModelHelper.CreateSuccessResponse(organizer));
        }

        [HttpPost("{eventId}/tickets")]
        public async Task<IActionResult> CreateTicketForEvent(int eventId, TicketCreateDto dto)
        {
            var ticket = await eventService.CreateTicketForEventAsync(eventId, dto);
            if (ticket is null)
                return NotFound(ResponseModelHelper.CreateNotFoundResponse<object>("Event not found"));
            return Ok(ResponseModelHelper.CreateSuccessResponse(ticket));
        }

    }
}
