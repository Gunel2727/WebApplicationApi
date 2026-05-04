using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Data;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizerController(ApiAppDbContext apiAppDbContext,IMapper mapper) : ControllerBase
    {
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await apiAppDbContext.Organizers
                 .Include(e => e.Events)
                 .ToListAsync();
            return Ok(events);
        }

    }
}
