using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using EventSphere.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventSphere.Infrastructure.Repositories
{
    public class WebsiteReviewRepository
    {
        private readonly AppDbContext _context;
        public WebsiteReviewRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<WebsiteReview>> GetAllReviewsAsync()
        {
            return await _context.WebsiteReviews.OrderByDescending(r => r.SubmittedAt).ToListAsync();
        }

        public async Task UpsertReviewAsync(WebsiteReview review)
        {
            var existing = await _context.WebsiteReviews.FirstOrDefaultAsync(r => r.UserId == review.UserId);
            if (existing != null)
            {
                existing.Rating = review.Rating;
                existing.Title = review.Title;
                existing.Comments = review.Comments;
                existing.SubmittedAt = System.DateTime.UtcNow;
                _context.WebsiteReviews.Update(existing);
            }
            else
            {
                review.SubmittedAt = System.DateTime.UtcNow;
                await _context.WebsiteReviews.AddAsync(review);
            }
            await _context.SaveChangesAsync();
        }
    }
}
