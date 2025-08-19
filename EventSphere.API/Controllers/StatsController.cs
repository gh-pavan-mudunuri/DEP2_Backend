using Microsoft.AspNetCore.Mvc;
using backend.Data;
using EventSphere.Application.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace EventSphere.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatsController : ControllerBase
    {
        private readonly IStatsService _statsService;
        public StatsController(IStatsService statsService)
        {
            _statsService = statsService;
        }

        [HttpGet("intro")]
        public async Task<IActionResult> GetIntroStats()
        {
            var users = await _statsService.GetUserCountAsync();
            var events = await _statsService.GetEventCountAsync();
            var registrations = await _statsService.GetRegistrationCountAsync();
            var locations = await _statsService.GetLocationCountAsync();
            var reviews = await _statsService.GetReviewCountAsync();
            var avgRating = await _statsService.GetAvgRatingAsync();

            return Ok(new {
                users,
                events,
                registrations,
                locations,
                reviews,
                avgRating
            });
        }
    }
}
