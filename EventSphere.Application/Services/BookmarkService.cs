using System.Threading.Tasks;
using EventSphere.Application.Dtos;
using EventSphere.Application.Interfaces;
using EventSphere.Application.Repositories;
using EventSphere.Domain.Entities;
using EventSphere.Domain.Enums;

namespace backend.Services
{
    public class BookmarkService : IBookmarkService
    {
        private readonly IBookmarkRepository _bookmarkRepository;
        private readonly IEventRepository _eventRepository;
        public BookmarkService(IBookmarkRepository bookmarkRepository, IEventRepository eventRepository)
        {
            _bookmarkRepository = bookmarkRepository;
            _eventRepository = eventRepository;
        }

        public async Task AddBookmarkAsync(AddBookmarkDto dto)
        {
            var bookmark = new Bookmark
            {
                UserId = dto.UserId,
                EventId = dto.EventId,
                UserEmail = dto.UserEmail,
                EventTitle = dto.EventTitle,
                CreatedAt = DateTime.UtcNow
            };

            await _bookmarkRepository.AddBookmarkAsync(bookmark);
        }

        public async Task DeleteBookmarkAsync(int bookmarkId)
        {
            await _bookmarkRepository.DeleteBookmarkAsync(bookmarkId);
        }

        public async Task DeleteBookmarkByUserAndEventAsync(int userId, int eventId)
        {
            await _bookmarkRepository.DeleteBookmarkByUserAndEventAsync(userId, eventId);
        }

        public async Task<IEnumerable<EventCardDto>> GetBookmarkedEventsByUserIdAsync(int userId)
        {
            var bookmarkedEventIds = await _bookmarkRepository.GetAllBookmarkEventsIdByUserIdAsync(userId);

            var events = new List<EventDto>();
            foreach (var eventId in bookmarkedEventIds)
            {
                var ev = await _eventRepository.GetEventByIdNewAsync(eventId);
                if (ev != null)
                {
                    events.Add(ev);
                }
            }

            var eventCardDtos = new List<EventCardDto>();

            foreach (var e in events)
            {
                var registrationCount = await _eventRepository.GetRegistrationCountForEventAsync(e.EventId);

                eventCardDtos.Add(new EventCardDto
                {
                    EventId = e.EventId,
                    Title = e.Title,
                    CoverImage = e.CoverImage,
                    Category = e.Category,
                    EventType = !string.IsNullOrEmpty(e.EventType) && Enum.TryParse<EventType>(e.EventType, out var eventTypeEnum) ? eventTypeEnum : EventType.TBA,
                    Location = e.Location,
                    RegistrationDeadline = e.RegistrationDeadline,
                    EventStart = e.EventStart,
                    EventEnd = e.EventEnd,
                    Price = e.Price,
                    RegistrationCount = registrationCount,
                    IsVerifiedByAdmin = e.IsVerifiedByAdmin
                });
            }

            // Sort by registration count descending
            return eventCardDtos.OrderByDescending(e => e.RegistrationCount).ToList();
        }
    }
}