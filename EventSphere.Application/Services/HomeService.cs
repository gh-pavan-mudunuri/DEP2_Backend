using EventSphere.Domain.Enums;
using backend.Interfaces;
using EventSphere.Domain.Entities;
using EventSphere.Application.Dtos;
using EventSphere.Application.Dtos.Events;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventSphere.Application.Repositories;
using EventSphere.Application.Helpers;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using EventSphere.Application.Mappers;

namespace backend.Services
{
    public class HomeService : IHomeService
    {

        private readonly IEventRepository _eventRepository;
        private readonly ILogger<HomeService> _logger;

        public HomeService(IEventRepository eventRepository, ILogger<HomeService> logger)
        {
            _eventRepository = eventRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<EventCardDto>> GetUpcomingEventsAsync()
        {
            var events = await _eventRepository.GetUpcomingEventsAsync();
            return events.Select(EventMapper.ToCardDto).ToList();
        }

        public async Task<IEnumerable<EventCardDto>> GetTrendingEventsAsync()
        {
            var events = await _eventRepository.GetTrendingEventsAsync();
            return events.Select(EventMapper.ToCardDto).ToList();
        }


        public async Task<IEnumerable<EventCardDto>> FilterEventsAsync(EventFilterDto filter)
        {
            var events = await _eventRepository.FilterEventsAsync(filter);

            return events;
        }



public async Task<(IEnumerable<EventCardDto> Events, int TotalCount)> FilterEventsPagedAsync(EventFilterDto filter, int page = 1, int pageSize = 20)
        {
            return await _eventRepository.FilterEventsPagedAsync(filter, page, pageSize);
        }


    }
}