using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Dtos.EventDtos;
using WebApplication2.Dtos.OrganizerDtos;
using WebApplication2.Dtos.TicketDtos;
using WebApplication2.Extensions;
using WebApplication2.Models;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController(IEventService eventService,ApiAppDbContext apiAppDbContext) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await eventService.GetAllEventsAsync();
            return Ok(events);
        }

        [HttpPost]
        public async Task<IActionResult> Post(EventCreateDto dto)
        {
            var ev = await eventService.CreateEventAsync(dto);
            return Ok(ev);
        }

        [HttpPost("{id}/banner")]
        public async Task<IActionResult> UploadBanner(int id, IFormFile file)
        {
            if (file == null)
                return BadRequest("File is required");

            if (!file.IsImage())
                return BadRequest("Only image files are allowed");

            if (!file.IsValidSize(2)) 
                return BadRequest("File size must be less than 2MB");

            var ev = await apiAppDbContext.Events.FindAsync(id);
            if (ev == null)
                return NotFound("Event not found");

            string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

           
            string fileName = await file.SaveFileAsync(rootPath);

            ev.BannerImageUrl = fileName;

            await apiAppDbContext.SaveChangesAsync();

            return Ok(new
            {
                message = "Banner uploaded",
                fileName = fileName,
                path = $"/images/{fileName}",
                url = $"{Request.Scheme}://{Request.Host}/images/{fileName}"
            });
        }

        [HttpGet("{eventId}/tickets")]
        public async Task<IActionResult> GetTicketsByEvent(int eventId)
        {
            var tickets = await eventService.GetTicketsByEventAsync(eventId);
            if (tickets is null) return NotFound("Event not found");
            return Ok(tickets);
        }

        [HttpGet("{eventId}/organizer")]
        public async Task<IActionResult> GetOrganizerByEvent(int eventId)
        {
            var organizer = await eventService.GetOrganizerByEventAsync(eventId);
            if (organizer is null) return NotFound("Not found");
            return Ok(organizer);
        }

        [HttpPost("{eventId}/tickets")]
        public async Task<IActionResult> CreateTicketForEvent(int eventId, TicketCreateDto dto)
        {
            var ticket = await eventService.CreateTicketForEventAsync(eventId, dto);
            if (ticket is null) return NotFound("Event not found");
            return Ok(ticket);
        }

    }
}
