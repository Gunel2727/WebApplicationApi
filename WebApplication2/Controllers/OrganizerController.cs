using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;
using WebApplication2.Dtos.EventDtos;
using WebApplication2.Dtos.OrganizerDtos;
using WebApplication2.Extensions;
using WebApplication2.Models;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizerController(IOrganizerService organizerService,ApiAppDbContext? apiAppDbContext) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAllOrganizers()
        {
            var organizers = await organizerService.GetAllOrganizersAsync();
            return Ok(organizers);
        }

        [HttpPost]
        public async Task<IActionResult> Create(OrganizerCreateDto dto)
        {
            var organizer = await organizerService.CreateOrganizerAsync(dto);
            return Ok(organizer);
        }

        [HttpPost("{id}/logo")]
        public async Task<IActionResult> UploadLogo(int id, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("File is required");
            if (!file.IsImage()) return BadRequest("Only image files are allowed");
            if (!file.IsValidSize(5)) return BadRequest("File size must not exceed 5MB");

            var organizer = await apiAppDbContext.Organizers.FindAsync(id);
            if (organizer == null) return NotFound("Organizer not found");

            string rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            if (!string.IsNullOrEmpty(organizer.LogoUrl))
            {
                var oldPath = Path.Combine(rootPath, organizer.LogoUrl);
                if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
            }

            organizer.LogoUrl = await file.SaveFileAsync(rootPath);
            await apiAppDbContext.SaveChangesAsync();
            return Ok(new
            {
                message = "Logo uploaded",
                fileName = organizer.LogoUrl,
                path = $"/images/{organizer.LogoUrl}",
                url = $"{Request.Scheme}://{Request.Host}/images/{organizer.LogoUrl}"
            });
        }

        [HttpGet("{organizerId}/events")]
        public async Task<IActionResult> GetEventsByOrganizer(int organizerId)
        {
            var events = await organizerService.GetEventsByOrganizerAsync(organizerId);
            if (events is null) return NotFound("Organizer not found");
            return Ok(events);
        }


    }
}
