using System.Collections.Generic;
using System.Threading.Tasks;
using EventSphere.Application.Dtos;

namespace EventSphere.Application.Interfaces
{
    public interface IBookmarkService
    {

        Task AddBookmarkAsync(AddBookmarkDto dto);
        Task DeleteBookmarkAsync(int bookmarkId);
        Task DeleteBookmarkByUserAndEventAsync(int userId, int eventId);
        Task<IEnumerable<EventCardDto>> GetBookmarkedEventsByUserIdAsync(int userId);
    }
}
