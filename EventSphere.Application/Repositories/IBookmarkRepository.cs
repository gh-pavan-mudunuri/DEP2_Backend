using System.Collections.Generic;
using System.Threading.Tasks;
using EventSphere.Domain.Entities;
using EventSphere.Application.Dtos;

namespace EventSphere.Application.Repositories
{
    public interface IBookmarkRepository
    {
        Task<IEnumerable<int>> GetAllBookmarkEventsIdByUserIdAsync(int id);
        Task AddBookmarkAsync(Bookmark bk);
        Task DeleteBookmarkAsync(int id);
        Task DeleteBookmarkByUserAndEventAsync(int userId, int eventId);
    }
}
