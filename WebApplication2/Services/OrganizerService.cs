using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Dtos.EventDtos;
using WebApplication2.Dtos.OrganizerDtos;
using WebApplication2.Models;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class OrganizerService(ApiAppDbContext db, IMapper mapper) : IOrganizerService
    {
        public async Task<List<OrganizerReturnDto>> GetAllOrganizersAsync()
        {
            var organizers = await db.Organizers
                .Include(o => o.Events)
                .ToListAsync();
            return mapper.Map<List<OrganizerReturnDto>>(organizers);
        }

        public async Task<OrganizerReturnDto> CreateOrganizerAsync(OrganizerCreateDto dto)
        {
            var organizer = mapper.Map<Organizer>(dto);
            db.Organizers.Add(organizer);
            await db.SaveChangesAsync();
            return mapper.Map<OrganizerReturnDto>(organizer);
        }

        public async Task<List<EventReturnDto>> GetEventsByOrganizerAsync(int organizerId)
        {
            var organizerExists = await db.Organizers.AnyAsync(o => o.Id == organizerId);
            if (!organizerExists) return null;
            var events = await db.Events
                .Where(e => e.OrganizerId == organizerId)
                .ToListAsync();
            return mapper.Map<List<EventReturnDto>>(events);
        }
    }
}
