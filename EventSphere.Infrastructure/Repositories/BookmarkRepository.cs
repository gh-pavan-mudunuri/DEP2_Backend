using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventSphere.Application.Repositories;
using EventSphere.Domain.Entities;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace EventSphere.Infrastructure.Repositories
{
    public class BookmarkRepository : IBookmarkRepository
    {
        private readonly AppDbContext _context;

        public BookmarkRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<int>> GetAllBookmarkEventsIdByUserIdAsync(int userId)
        {
            return await _context.Bookmarks
                .Where(b => b.UserId == userId)
                .Select(b => b.EventId)
                .ToListAsync();
        }

        public async Task AddBookmarkAsync(Bookmark bk)
        {
            _context.Bookmarks.Add(bk);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteBookmarkAsync(int bookmarkId)
        {
            var bookmark = await _context.Bookmarks.FindAsync(bookmarkId);
            if (bookmark != null)
            {
                _context.Bookmarks.Remove(bookmark);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteBookmarkByUserAndEventAsync(int userId, int eventId)
        {
            var bookmark = await _context.Bookmarks.FirstOrDefaultAsync(b => b.UserId == userId && b.EventId == eventId);
            if (bookmark != null)
            {
                _context.Bookmarks.Remove(bookmark);
                await _context.SaveChangesAsync();
            }
        }
    }
}
