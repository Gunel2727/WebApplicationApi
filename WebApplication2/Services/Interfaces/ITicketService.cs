using WebApplication2.Dtos.TicketDtos;

namespace WebApplication2.Services.Interfaces
{
    public interface ITicketService
    {
        Task<List<TicketReturnDto>> GetAllTicketsAsync();
        Task<TicketReturnDto?> CreateTicketAsync(TicketCreateDto dto);
    }
}
