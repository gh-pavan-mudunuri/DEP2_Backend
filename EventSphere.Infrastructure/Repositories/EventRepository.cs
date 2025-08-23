using System.Collections.Generic;
using System.Threading.Tasks;
using EventSphere.Application.Repositories;
using EventSphere.Domain.Entities;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using EventSphere.Application.Dtos;
using EventSphere.Domain.Enums;

namespace EventSphere.Infrastructure.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly AppDbContext _context;
        public EventRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Event?> GetEventByIdAsync(int id)
        {
            return await _context.Events
                .Include(e => e.Speakers)
                .Include(e => e.Occurrences)
                .Include(e => e.Faqs)
                .Include(e => e.Media)
                .Include(e => e.Registrations)
                .FirstOrDefaultAsync(e => e.EventId == id);
        }


        public async Task<(IEnumerable<EventCardDto> Events, int TotalCount)> GetUnapprovedEventsPagedAsync(int page, int pageSize)
        {
            var query = _context.Events
                .Where(e => !e.IsVerifiedByAdmin)
                .Select(e => new EventCardDto
                {
                    EventId = e.EventId,
                    Title = e.Title,
                    CoverImage = e.CoverImage,
                    Category = e.Category,
                    EventType = e.EventType,
                    Location = e.Location,
                    RegistrationDeadline = e.RegistrationDeadline,
                    EventStart = e.EventStart,
                    EventEnd = e.EventEnd,
                    Price = e.Price,
                    IsVerifiedByAdmin = e.IsVerifiedByAdmin,
                    RegistrationCount = e.Registrations != null ? e.Registrations.Sum(r => r.TicketCount) : 0,
                    OrganizerEmail = e.OrganizerEmail

                });

            var totalCount = await query.CountAsync();
            var events = await query
                .OrderByDescending(e => e.EventStart)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (events, totalCount);
        }


        public async Task<EventDto?> GetEventByIdNewAsync(int id)
        {
            // Get the event (scalar fields only)
            var evnt = await _context.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (evnt == null)
                return null;

            // FAQs
            var faqs = await _context.EventFaqs
                .AsNoTracking()
                .Where(f => f.EventId == id)
                .Select(f => new EventFaqDto
                {
                    FaqId = f.FaqId,
                    Question = f.Question,
                    Answer = f.Answer
                })
                .ToListAsync();

            // Speakers
            var speakers = await _context.EventSpeakers
                .AsNoTracking()
                .Where(s => s.EventId == id)
                .Select(s => new EventSpeakerDto
                {
                    SpeakerId = s.SpeakerId,
                    Name = s.Name,
                    Bio = s.Bio,
                    PhotoUrl = s.PhotoUrl
                })
                .ToListAsync();

            // Occurrences
            var occurrences = await _context.EventOccurrences
                .AsNoTracking()
                .Where(o => o.EventId == id)
                .Select(o => new EventOccurrenceDto
                {
                    OccurrenceId = o.OccurrenceId,
                    StartTime = o.StartTime,
                    EndTime = o.EndTime,
                    EventTitle = o.EventTitle,
                    IsCancelled = o.IsCancelled
                })
                .ToListAsync();

            // Media
            var media = await _context.EventMedias
                .AsNoTracking()
                .Where(m => m.EventId == id)
                .Select(m => new EventMediaDto
                {
                    MediaId = m.MediaId,
                    MediaUrl = m.MediaUrl,
                    MediaType = m.MediaType.ToString(),
                    Description = m.Description
                })
                .ToListAsync();

            // Registrations count
            var registrationCount = await _context.Registrations
                .AsNoTracking()
                .CountAsync(r => r.EventId == id);

            // Build DTO
            var result = new EventDto
            {
                EventId = evnt.EventId,
                OrganizerId = evnt.OrganizerId,
                Title = evnt.Title,
                CoverImage = evnt.CoverImage,
                VibeVideoUrl = evnt.VibeVideoUrl,
                Category = evnt.Category,
                Location = evnt.Location,
                EventType = evnt.EventType.ToString(),
                RegistrationDeadline = evnt.RegistrationDeadline,
                EventStart = evnt.EventStart,
                EventEnd = evnt.EventEnd,
                IsRecurring = evnt.IsRecurring,
                RecurrenceType = evnt.RecurrenceType,
                RecurrenceRule = evnt.RecurrenceRule,
                CustomFields = evnt.CustomFields,
                Description = evnt.Description,
                IsPaidEvent = evnt.IsPaidEvent,
                Price = evnt.Price,
                EventLink = evnt.EventLink,
                OrganizerName = evnt.OrganizerName,
                OrganizerEmail = evnt.OrganizerEmail,
                MaxAttendees = evnt.MaxAttendees,
                EditEventCount = evnt.EditEventCount,
                Occurrences = occurrences,
                Speakers = speakers,
                Faqs = faqs,
                Media = media,
                RegistrationCount = registrationCount,
                IsVerifiedByAdmin = evnt.IsVerifiedByAdmin,
                AdminVerifiedAt = evnt.AdminVerifiedAt ?? DateTime.MinValue
            };

            return result;
        }



        private EventDto? NotFound()
        {
            throw new NotImplementedException();
        }

        private EventDto? Ok(object result)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            return await _context.Events
                .Include(e => e.Speakers)
                .Include(e => e.Occurrences)
                .Include(e => e.Registrations)
                .ToListAsync();
        }

        public async Task<List<EventCardDto>> GetCurrentOrganizedEventsAsync(int organizerId)
        {
            return await _context.Events
                .Where(e => e.OrganizerId == organizerId && e.IsCompleted == 0)
                .Select(e => new EventCardDto
                {
                    EventId = e.EventId,
                    Title = e.Title,
                    CoverImage = e.CoverImage,
                    Category = e.Category,
                    EventType = e.EventType,
                    Location = e.Location,
                    EditEventCount = e.EditEventCount,
                    RegistrationDeadline = e.RegistrationDeadline,
                    EventStart = e.EventStart,
                    EventEnd = e.EventEnd,
                    Price = e.Price,
                    IsVerifiedByAdmin = e.IsVerifiedByAdmin,
                    RegistrationCount = e.Registrations != null ? e.Registrations.Sum(r => r.TicketCount) : 0
                })
                .ToListAsync();
        }


        public async Task<List<Event>> GetUpcomingEventsAsync()
        {
            var now = DateTime.UtcNow;

            return await _context.Events
                .Where(e => e.EventStart >= now && e.IsVerifiedByAdmin)
                .Include(e => e.Speakers)
                .Include(e => e.Occurrences)
                .Include(e => e.Registrations)
                .OrderBy(e => e.EventStart)
                .Take(10) // Optional: adjust based on UI needs
                .ToListAsync();
        }

        public async Task<List<Event>> GetTrendingEventsAsync()
        {
            var now = DateTime.UtcNow;

            var events = await _context.Events
                .Where(e => e.EventStart >= now && e.IsVerifiedByAdmin)
                .Include(e => e.Registrations)
                .ToListAsync();

            return events
                .OrderByDescending(e => e.Registrations != null ? e.Registrations.Sum(r => r.TicketCount) : 0)
                .Take(10)
                .ToList();
        }



        public async Task<(IEnumerable<Event> Events, int TotalCount)> GetEventsPagedAsync(int page, int pageSize)
        {
            var query = _context.Events
                .Include(e => e.Speakers)
                .Include(e => e.Occurrences)
                .Include(e => e.Registrations);

            var totalCount = await query.CountAsync();
            var events = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (events, totalCount);
        }

        public async Task AddEventAsync(Event ev)
        {
            _context.Events.Add(ev);
            // Explicitly add EventMedia to context to guarantee persistence
            if (ev.Media != null)
            {
                foreach (var media in ev.Media)
                {
                    _context.EventMedias.Add(media);
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task UpdateEventAsync(Event ev)
        {
            // --- Update Occurrences: Remove all and re-add new, resetting OccurrenceId ---
            if (ev.Occurrences != null)
            {
                var existingOccurrences = _context.EventOccurrences.Where(o => o.EventId == ev.EventId);
                _context.EventOccurrences.RemoveRange(existingOccurrences);
                foreach (var occ in ev.Occurrences)
                {
                    occ.EventId = ev.EventId;
                    occ.OccurrenceId = 0; // Prevent identity insert error
                    _context.EventOccurrences.Add(occ);
                }
            }

            // --- Update Speakers ---
            if (ev.Speakers != null)
            {
                var existingSpeakers = _context.EventSpeakers.Where(s => s.EventId == ev.EventId);
                _context.EventSpeakers.RemoveRange(existingSpeakers);
                foreach (var speaker in ev.Speakers)
                {
                    speaker.EventId = ev.EventId;
                    speaker.SpeakerId = 0; // Prevent identity insert error
                    _context.EventSpeakers.Add(speaker);
                }
            }

            // --- Update FAQs ---
            if (ev.Faqs != null)
            {
                var existingFaqs = _context.EventFaqs.Where(f => f.EventId == ev.EventId);
                _context.EventFaqs.RemoveRange(existingFaqs);
                foreach (var faq in ev.Faqs)
                {
                    faq.EventId = ev.EventId;
                    faq.FaqId = 0; // Prevent identity insert error
                    _context.EventFaqs.Add(faq);
                }
            }

            _context.Events.Update(ev);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteEventAsync(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null)
            {
                return false;
            }
            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetRegistrationCountForEventAsync(int eventId)
        {
            // Return the total number of tickets booked for the event (sum of TicketCount)
            return await _context.Registrations.Where(r => r.EventId == eventId).SumAsync(r => r.TicketCount);
        }

        public async Task<IEnumerable<EventCardDto>> FilterEventsAsync(EventFilterDto filter)
        {
            var query = _context.Events.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.Location))
            {
                var pattern = $"%{filter.Location.ToLower()}%";
                query = query.Where(e => EF.Functions.Like((e.Location ?? "").ToLower(), pattern));
            }

            if (!string.IsNullOrEmpty(filter.Price))
            {
                if (filter.Price.Equals("free", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(e => e.Price == 0 || e.Price == null);
                }
                else if (filter.Price.Equals("paid", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(e => e.Price > 0);
                }
            }

            if (filter.Date.HasValue)
                query = query.Where(e => filter.Date.Value >= e.EventStart && filter.Date.Value <= e.EventEnd);

            if (!string.IsNullOrEmpty(filter.Category))
            {
                var standardCategories = new List<string>
        {
            "Music", "Tech", "Health", "Education", "Business", "Conference", "Exhibitions"
        };

                if (filter.Category.Equals("others", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(e => e.Category != null && !standardCategories.Contains(e.Category));
                }
                else
                {
                    query = query.Where(e => e.Category == filter.Category);
                }
            }

            if (!string.IsNullOrEmpty(filter.RecurrenceType))
            {
                if (filter.RecurrenceType.Equals("onetime", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(e => e.RecurrenceType == "None");
                }
                else if (filter.RecurrenceType.Equals("multiple", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(e => e.RecurrenceType != null && e.RecurrenceType != "None");
                }
                else
                {
                    query = query.Where(e => e.RecurrenceType == filter.RecurrenceType);
                }
            }

            if (!string.IsNullOrEmpty(filter.EventType))
            {
                if (filter.EventType.Equals("online", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(e => e.EventType == EventSphere.Domain.Enums.EventType.Online);
                }
                else if (filter.EventType.Equals("offline", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(e => e.EventType != EventSphere.Domain.Enums.EventType.Online);
                }
            }

            var now = DateTime.UtcNow;
            query = query.Where(e => e.EventStart >= now)
                         .OrderBy(e => e.EventStart);

            return await query
                .Select(e => new EventCardDto
                {
                    EventId = e.EventId,
                    Title = e.Title,
                    CoverImage = e.CoverImage ?? string.Empty,
                    Category = e.Category ?? string.Empty,
                    EventType = e.EventType,
                    Location = e.Location,
                    EditEventCount = e.EditEventCount,
                    RegistrationDeadline = e.RegistrationDeadline,
                    EventStart = e.EventStart,
                    EventEnd = e.EventEnd,
                    Price = e.Price,
                    RegistrationCount = e.Registrations != null ? e.Registrations.Count : 0, // ✅ COUNT of related registrations
                    IsVerifiedByAdmin = e.IsVerifiedByAdmin,
                    OrganizerEmail = e.OrganizerEmail
                })
                .ToListAsync();
        }


        public async Task<Dictionary<int, int>> GetRegistrationCountsForEventsAsync(List<int> eventIds)
        {
            return await _context.Registrations
                .Where(r => eventIds.Contains(r.EventId))
                .GroupBy(r => r.EventId)
                .Select(g => new
                {
                    EventId = g.Key,
                    Count = g.Count()
                })
                .ToDictionaryAsync(g => g.EventId, g => g.Count);
        }

        public async Task<int?> GetEventEditCountAsync(int eventId)
        {
            return await _context.Events
                .Where(e => e.EventId == eventId)
                .Select(e => (int?)e.EditEventCount)
                .FirstOrDefaultAsync();
        }



        // Get all registrations for an event (for notifications)
        public async Task<List<Registration>> GetRegistrationsByEventIdAsync(int eventId)
        {
            return await _context.Registrations
                .Where(r => r.EventId == eventId)
                .ToListAsync();
        }

        public async Task<List<int>> GetRegisteredEventIdsByUserIdAsync(int userId)
        {
            return await _context.Registrations
                .Where(r => r.UserId == userId)
                .Select(r => r.EventId)
                .Distinct()
                .ToListAsync();
        }
        

        public async Task<(IEnumerable<EventCardDto> Events, int TotalCount)> FilterEventsPagedAsync(EventFilterDto filter, int page = 1, int pageSize = 20)
{
    var query = _context.Events.AsNoTracking().AsQueryable();

    // Apply filters
    if (!string.IsNullOrEmpty(filter.Location))
    {
        var pattern = $"%{filter.Location.ToLower()}%";
        query = query.Where(e => EF.Functions.Like((e.Location ?? "").ToLower(), pattern));
    }
    if (!string.IsNullOrEmpty(filter.Price))
    {
        if (filter.Price.Equals("free", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(e => e.Price == 0 || e.Price == null);
        }
        else if (filter.Price.Equals("paid", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(e => e.Price > 0);
        }
    }
    if (filter.Date.HasValue)
        query = query.Where(e => filter.Date.Value >= e.EventStart && filter.Date.Value <= e.EventEnd);
    if (!string.IsNullOrEmpty(filter.Category))
    {
        var standardCategories = new List<string>
        {
            "Music", "Tech", "Health", "Education", "Business", "Conference", "Exhibitions"
        };
        if (filter.Category.Equals("others", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(e => e.Category != null && !standardCategories.Contains(e.Category));
        }
        else
        {
            query = query.Where(e => e.Category == filter.Category);
        }
    }
    if (!string.IsNullOrEmpty(filter.RecurrenceType))
    {
        if (filter.RecurrenceType.Equals("onetime", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(e => e.RecurrenceType == "None");
        }
        else if (filter.RecurrenceType.Equals("multiple", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(e => e.RecurrenceType != null && e.RecurrenceType != "None");
        }
        else
        {
            query = query.Where(e => e.RecurrenceType == filter.RecurrenceType);
        }
    }
    if (!string.IsNullOrEmpty(filter.EventType))
    {
        if (filter.EventType.Equals("online", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(e => e.EventType == EventType.Online);
        }
        else if (filter.EventType.Equals("offline", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(e => e.EventType != EventType.Online);
        }
    }
    var now = DateTime.UtcNow;
    query = query.Where(e => e.EventStart >= now)
                 .OrderBy(e => e.EventStart);
    var totalCount = await query.CountAsync();
    var events = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(e => new EventCardDto
        {
            EventId = e.EventId,
            Title = e.Title,
            CoverImage = e.CoverImage ?? string.Empty,
            Category = e.Category ?? string.Empty,
            EventType = e.EventType,
            Location = e.Location,
            EditEventCount = e.EditEventCount,
            RegistrationDeadline = e.RegistrationDeadline,
            EventStart = e.EventStart,
            EventEnd = e.EventEnd,
            Price = e.Price,
            RegistrationCount = e.Registrations != null ? e.Registrations.Sum(r => r.TicketCount) : 0,
            IsVerifiedByAdmin = e.IsVerifiedByAdmin,
            OrganizerEmail = e.OrganizerEmail
        })
        .ToListAsync();
    return (events, totalCount);
}
    }
}
