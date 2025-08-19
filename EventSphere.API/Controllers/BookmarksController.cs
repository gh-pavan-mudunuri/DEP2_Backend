using Microsoft.AspNetCore.Mvc;
using EventSphere.Domain.Entities;
using EventSphere.Application.Interfaces;
using EventSphere.Application.Dtos;

namespace EventSphere.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookmarksController : ControllerBase
    {

        private readonly IBookmarkService _bookmarkService;

        public BookmarksController(IBookmarkService bookmarkService)
        {
            _bookmarkService = bookmarkService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddBookmark([FromBody] AddBookmarkDto dto)
        {
            if (dto == null) return BadRequest("Invalid bookmark data.");

            await _bookmarkService.AddBookmarkAsync(dto);
            return Ok("Bookmark added successfully.");
        }

        [HttpDelete("delete/{eventId}")]
        public async Task<IActionResult> DeleteBookmark(int eventId, [FromQuery] int userId)
        {
            await _bookmarkService.DeleteBookmarkByUserAndEventAsync(userId, eventId);
            return Ok("Bookmark deleted successfully.");
        }

        [HttpGet("bookmarked-events/{userId}")]
        public async Task<IActionResult> GetBookmarkedEvents(int userId)
        {
            var events = await _bookmarkService.GetBookmarkedEventsByUserIdAsync(userId);
            // Always return 200 OK with an array (empty if no bookmarks)
            return Ok((events ?? new List<EventSphere.Application.Dtos.EventCardDto>()).ToList());
        }
    }
}
