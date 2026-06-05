using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Dtos;
using WebApplication2.Dtos.EventDtos;
using WebApplication2.Dtos.OrganizerDtos;
using WebApplication2.Extensions;
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
            if (dto.Logo != null)
            {
                string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                string fileName = await dto.Logo.SaveFileAsync(rootPath);
                organizer.LogoUrl = $"https://localhost:7268/images/{fileName}";
            }
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
        public async Task<UploadLogoReturnDto?> UploadLogoAsync(int id, IFormFile file, string requestScheme, string requestHost)
        {
            var organizer = await db.Organizers.FindAsync(id);
            if (organizer == null) return null;

            string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

            if (!string.IsNullOrEmpty(organizer.LogoUrl))
            {
                var oldPath = Path.Combine(rootPath, organizer.LogoUrl);
                if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
            }

            organizer.LogoUrl = await file.SaveFileAsync(rootPath);
            await db.SaveChangesAsync();

            return new UploadLogoReturnDto
            {
                FileName = organizer.LogoUrl,
                Path = $"/images/{organizer.LogoUrl}",
                Url = $"{requestScheme}://{requestHost}/images/{organizer.LogoUrl}"
            };
        }
    }
}
