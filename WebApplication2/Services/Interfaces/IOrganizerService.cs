using WebApplication2.Dtos.EventDtos;
using WebApplication2.Dtos.OrganizerDtos;

namespace WebApplication2.Services.Interfaces
{
    public interface IOrganizerService
    {
        Task<List<OrganizerReturnDto>> GetAllOrganizersAsync();
        Task<OrganizerReturnDto> CreateOrganizerAsync(OrganizerCreateDto dto);
        Task<List<EventReturnDto>> GetEventsByOrganizerAsync(int organizerId);
    }
}
