using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Dtos.TicketDtos;
using WebApplication2.Models;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class TicketService(ApiAppDbContext db, IMapper mapper) : ITicketService
    {
        public async Task<List<TicketReturnDto>> GetAllTicketsAsync()
        {
            var tickets = await db.Tickets
                .Include(t => t.Event)
                .ToListAsync();
            return mapper.Map<List<TicketReturnDto>>(tickets);
        }

        public async Task<TicketReturnDto?> CreateTicketAsync(TicketCreateDto dto)
        {
            var eventExists = await db.Events.AnyAsync(e => e.Id == dto.EventId);
            if (!eventExists) return null;
            var ticket = mapper.Map<Ticket>(dto);
            db.Tickets.Add(ticket);
            await db.SaveChangesAsync();
            return mapper.Map<TicketReturnDto>(ticket);
        }
    }
}
