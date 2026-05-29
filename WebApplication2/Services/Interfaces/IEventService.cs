using WebApplication2.Dtos.EventDtos;
using WebApplication2.Dtos.OrganizerDtos;
using WebApplication2.Dtos.TicketDtos;

namespace WebApplication2.Services.Interfaces
{
    public interface IEventService
    {
        Task<List<EventReturnDto>> GetAllEventsAsync();
        Task<EventReturnDto> CreateEventAsync(EventCreateDto dto);
        Task<List<TicketReturnDto>> GetTicketsByEventAsync(int eventId);
        Task<OrganizerReturnDto> GetOrganizerByEventAsync(int eventId);
        Task<TicketReturnDto> CreateTicketForEventAsync(int eventId, TicketCreateDto dto);
    }
}
