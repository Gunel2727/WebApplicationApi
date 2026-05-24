using AutoMapper;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Controllers;
using WebApplication2.Data;
using WebApplication2.Dtos.EventDtos;
using WebApplication2.Dtos.OrganizerDtos;
using WebApplication2.Dtos.TicketDtos;
using WebApplication2.Models;

namespace TestWebProject
{
    public class OrganizerControllerTests
    {
        private readonly ApiAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly OrganizerController _controller;

        public OrganizerControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApiAppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new ApiAppDbContext(options);
            var mapper = new Mapper(new MapperConfiguration(
             cfg =>
             {
                 cfg.CreateMap<EventCreateDto, Event>();
                 cfg.CreateMap<TicketCreateDto, Ticket>();
                 cfg.CreateMap<OrganizerCreateDto, Organizer>();
                 cfg.CreateMap<Organizer, OrganizerReturnDto>();
                 cfg.CreateMap<Organizer, OrganizerInEventDto>();
                 cfg.CreateMap<Ticket, TicketReturnDto>();
                 cfg.CreateMap<Event, EventInTicketDto>();
                 cfg.CreateMap<Event, EventReturnDto>()
                     .ForMember(dest => dest.BannerImageUrl, opt => opt.Ignore())
                     .ForMember(dest => dest.Organizer, opt => opt.MapFrom(src => src.Organizer));
                 cfg.CreateMap<Event, EventInOrganizerDto>()
                     .ForMember(dest => dest.BannerImageUrl, opt => opt.Ignore());
             },
             Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance
                 ));

            _mapper = mapper;

            _controller = new OrganizerController(_db, _mapper);



        }
        [Fact]
        public async Task GetAllOrganizers_ReturnsOkWithList()
        {
            _db.Organizers.Add(new Organizer { Id = 1, Name = "Test Org", Email = "test@test.com" });
            await _db.SaveChangesAsync();

            var result = await _controller.GetAllOrganizers();

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var list = ok.Value.Should().BeAssignableTo<List<OrganizerReturnDto>>().Subject;
            list.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetEventsByOrganizer_OrganizerNotFound_ReturnsNotFound()
        {
            var result = await _controller.GetEventsByOrganizer(999);
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetEventsByOrganizer_ReturnsOnlyThatOrganizersEvents()
        {
            _db.Organizers.Add(new Organizer { Id = 1, Name = "Test Org", Email = "test@test.com" });
            _db.Events.AddRange(
                 new Event { Id = 1, OrganizerId = 1, Title = "Event 1", Location = "Baku" },
                 new Event { Id = 2, OrganizerId = 2, Title = "Event 2", Location = "Baku" }
                 );
            await _db.SaveChangesAsync();

            var result = await _controller.GetEventsByOrganizer(1);

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            var events = ok.Value.Should().BeAssignableTo<List<EventReturnDto>>().Subject;
            events.Should().HaveCount(1);
        }

        [Fact]
        public async Task UploadLogo_OrganizerNotFound_ReturnsNotFound()
        {
            var file = CreateFakeImage("logo.jpg");

            var result = await _controller.UploadLogo(999, file);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task UploadLogo_NullFile_ReturnsBadRequest()
        {
            var result = await _controller.UploadLogo(1, null);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        private IFormFile CreateFakeImage(string fileName)
        {
            var bytes = new byte[1024];
            var stream = new MemoryStream(bytes);
            return new FormFile(stream, 0, bytes.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };
        }

    }
}
