using AutoMapper;
using WebApplication2.Dtos.EventDtos;
using WebApplication2.Models;

namespace WebApplication2.Profiles
{
    public class MapperProfile:Profile
    {
        public MapperProfile()
        {
           
            CreateMap<EventCreateDto,Event>();
            CreateMap<TicketCreateDto,Ticket>();
          
        }
    }
}
