using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Dtos.EventDtos;
using WebApplication2.Dtos.OrganizerDtos;
using WebApplication2.Dtos.TicketDtos;
using WebApplication2.Extensions;
using WebApplication2.Models;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class EventService(ApiAppDbContext db, IMapper mapper) : IEventService
    {
        public async Task<List<EventReturnDto>> GetAllEventsAsync()
        {
            var events = await db.Events
                .Include(e => e.Organizer)
                .ToListAsync();
            return mapper.Map<List<EventReturnDto>>(events);
        }

        public async Task<EventReturnDto> CreateEventAsync(EventCreateDto dto)
        {
            var ev = mapper.Map<Event>(dto);
            ev.Date = dto.Date ?? DateTime.UtcNow;
            if (dto.Photo != null)
            {
                string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                string fileName = await dto.Photo.SaveFileAsync(rootPath);
                ev.BannerImageUrl = $"{fileName}";
            }
            db.Events.Add(ev);
            await db.SaveChangesAsync();
            return mapper.Map<EventReturnDto>(ev);
        }

        public async Task<List<TicketReturnDto>> GetTicketsByEventAsync(int eventId)
        {
            var eventExists = await db.Events.AnyAsync(e => e.Id == eventId);
            if (!eventExists) return null;
            var tickets = await db.Tickets
                .Where(t => t.EventId == eventId)
                .ToListAsync();
            return mapper.Map<List<TicketReturnDto>>(tickets);
        }

        public async Task<OrganizerReturnDto> GetOrganizerByEventAsync(int eventId)
        {
            var ev = await db.Events
                .Include(e => e.Organizer)
                .FirstOrDefaultAsync(e => e.Id == eventId);
            if (ev == null || ev.Organizer == null) return null;
            return mapper.Map<OrganizerReturnDto>(ev.Organizer);
        }

        public async Task<TicketReturnDto> CreateTicketForEventAsync(int eventId, TicketCreateDto dto)
        {
            var eventExists = await db.Events.AnyAsync(e => e.Id == eventId);
            if (!eventExists) return null;
            var ticket = new Ticket
            {
                EventId = eventId,
                Type = dto.Type,
                Price = dto.Price,
                QuantityAvailable = dto.QuantityAvailable
            };
            db.Tickets.Add(ticket);
            await db.SaveChangesAsync();
            return mapper.Map<TicketReturnDto>(ticket);
        }
    }
}
