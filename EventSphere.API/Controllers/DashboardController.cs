using Microsoft.AspNetCore.Mvc;
using EventSphere.Domain.Entities;
using EventSphere.Application.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventSphere.Application.Dtos;

namespace EventSphere.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }


        [HttpGet("organized-events-payments/{userId}")]
        public async Task<IActionResult> GetOrganizedEventsPayments(int userId)
        {
            var result = await _dashboardService.GetOrganizedEventsPaymentsAsync(userId);
            return Ok(result);
        }

        [HttpPost("current-organized/{id}")]
        public async Task<ActionResult<IEnumerable<Event>>> GetCurrentOrganizedEvents(int id)
        {
            var events = await _dashboardService.GetCurrentOrganizedEventsAsync(id);
            return Ok(events);
        }

        [HttpPost("past-organized/{id}")]
        public async Task<ActionResult<IEnumerable<Event>>> GetPastOrganizedEvents(int id)
        {
            var events = await _dashboardService.GetPastOrganizedEventsAsync(id);
            return Ok(events);
        }

        [HttpPost("current-attending/{id}")]
public async Task<ActionResult<IEnumerable<EventCardDto>>> GetCurrentAttendingEvents(int id)
{
    var events = await _dashboardService.GetCurrentAttendingEventsAsync(id);
    return Ok(events);
}

[HttpPost("past-attended/{id}")]
public async Task<ActionResult<IEnumerable<EventCardDto>>> GetPastAttendedEvents(int id)
{
    var events = await _dashboardService.GetPastAttendedEventsAsync(id);
    return Ok(events);
}
        
    
    }
}
