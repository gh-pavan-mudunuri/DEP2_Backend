using EventSphere.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using EventSphere.Application.Interfaces;

namespace EventSphere.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebsiteReviewController : ControllerBase
    {
        private readonly IWebsiteReviewService _reviewService;
        public WebsiteReviewController(IWebsiteReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // POST: api/WebsiteReview
        [HttpPost]
        public async Task<IActionResult> UpsertReview([FromBody] WebsiteReview review)
        {
            if (review == null || review.UserId == 0)
                return BadRequest(new { message = "Missing user info." });
            await _reviewService.UpsertReviewAsync(review);
            return StatusCode(201, new { message = "Review saved." });
        }

        // GET: api/WebsiteReview
        [HttpGet]
        public async Task<IActionResult> GetAllReviews()
        {
            var reviews = await _reviewService.GetAllReviewsAsync();
            return Ok(reviews);
        }
    }
}
