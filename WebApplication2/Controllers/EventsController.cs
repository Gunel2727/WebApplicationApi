using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Dtos.EventDtos;
using WebApplication2.Dtos.TicketDtos;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController(ApiAppDbContext apiAppDbContext,IMapper mapper) : ControllerBase
    {
       public async Task<IActionResult> GetAllEvents()
       {
           var events = await apiAppDbContext.Events
                .Include(e => e.Organizer)
                .ToListAsync();
           return Ok(events);
        }

        [HttpPost]
        public async Task<IActionResult> Create(EventCreateDto dto)
        {
            var ev = mapper.Map<Event>(dto);
            apiAppDbContext.Events.Add(ev);
            await apiAppDbContext.SaveChangesAsync();
            return Ok(ev);
        }

        [HttpPost("{id}/banner")]
        public async Task<IActionResult> UploadBanner(int id, IFormFile file)
        {
            var ev = await apiAppDbContext.Events.FindAsync(id);
            if (ev == null) return NotFound("Event not found");

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var path = Path.Combine("wwwroot/images", fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            ev.BannerImageUrl = fileName;
            await apiAppDbContext.SaveChangesAsync();

            return Ok(new { message = "Banner uploaded", file = fileName });
        }

        [HttpGet("{eventId}/tickets")]
        public async Task<IActionResult> GetTickets(int eventId)
        {
            var tickets = await apiAppDbContext.Tickets
                .Where(t => t.EventId == eventId)
                .ToListAsync();

            return Ok(tickets);
        }
        [HttpGet("{eventId}/organizer")]
        public async Task<IActionResult> GetOrganizer(int eventId)
        {
            var ev = await apiAppDbContext.Events
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (ev == null) return NotFound();

            return Ok(ev.Organizer);
        }
        [HttpPost("{eventId}/tickets")]
        public async Task<IActionResult> CreateTicket(int eventId, TicketCreateDto dto)
        {
            var ev = await apiAppDbContext.Events.FindAsync(eventId);
            if (ev == null) return NotFound("Event not found");

            var ticket = new Ticket
            {
                EventId = eventId,
                Type = dto.Type,
                Price = dto.Price,
                QuantityAvailable = dto.QuantityAvailable
            };

            apiAppDbContext.Tickets.Add(ticket);
            await apiAppDbContext.SaveChangesAsync();

            return Ok(ticket);
        }


    }
}
