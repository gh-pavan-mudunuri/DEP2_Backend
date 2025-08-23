using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EventSphere.Application.Dtos;


namespace EventSphere.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class HomeController : ControllerBase
    {
        private readonly backend.Interfaces.IHomeService _homeService;

        public HomeController(backend.Interfaces.IHomeService homeService)
        {
            _homeService = homeService;
        }

        [HttpGet("upcoming")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUpcomingEvents()
        {
            var events = await _homeService.GetUpcomingEventsAsync();
            return Ok(new { success = true, data = events });
        }

        [HttpGet("trending")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTrendingEvents()
        {
            var events = await _homeService.GetTrendingEventsAsync();
            return Ok(new { success = true, data = events });
        }

        [HttpPost("filter")]
        [AllowAnonymous]
        public async Task<IActionResult> FilterEvents([FromBody] EventFilterDto filter)
        {
            var events = await _homeService.FilterEventsAsync(filter);
            return Ok(new { success = true, data = events });
        }


[HttpPost("filter-paged")]
        [AllowAnonymous]
        public async Task<IActionResult> FilterEventsPaged([FromBody] EventFilterDto filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var (events, totalCount) = await _homeService.FilterEventsPagedAsync(filter, page, pageSize);
            return Ok(new { success = true, data = events, totalCount });
        }
    }
}