using EventSphere.Domain.Entities;
using EventSphere.Application.Dtos;
using System.Linq;

namespace EventSphere.Application.Mappers
{
    public static class EventMapper
    {
        public static EventCardDto ToCardDto(Event evt)
        {
            return new EventCardDto
            {
                EventId = evt.EventId,
                Title = evt.Title,
                CoverImage = evt.CoverImage,
                Category = evt.Category,
                EventType = evt.EventType,
                Location = evt.Location,
                RegistrationDeadline = evt.RegistrationDeadline,
                EventStart = evt.EventStart,
                EventEnd = evt.EventEnd,
                Price = evt.Price,
                RegistrationCount = evt.Registrations?.Sum(r => r.TicketCount) ?? 0,
                IsVerifiedByAdmin = evt.IsVerifiedByAdmin,
                OrganizerEmail = evt.OrganizerEmail
            };
        }

        public static EventDto ToDto(Event evt)
        {
            return new EventDto
            {
                EventId = evt.EventId,
                OrganizerId = evt.OrganizerId,
                Title = evt.Title,
                CoverImage = evt.CoverImage,
                VibeVideoUrl = evt.VibeVideoUrl,
                Category = evt.Category,
                Location = evt.Location,
                EventType = evt.EventType.ToString(),
                RegistrationDeadline = evt.RegistrationDeadline,
                EventStart = evt.EventStart,
                EventEnd = evt.EventEnd,
                IsRecurring = evt.IsRecurring,
                RecurrenceType = evt.RecurrenceType,
                RecurrenceRule = evt.RecurrenceRule,
                CustomFields = evt.CustomFields,
                Description = evt.Description,
                IsPaidEvent = evt.IsPaidEvent,
                Price = evt.Price,
                EventLink = evt.EventLink,
                OrganizerName = evt.OrganizerName,
                OrganizerEmail = evt.OrganizerEmail,
                MaxAttendees = evt.MaxAttendees,
                RegistrationCount = evt.Registrations?.Sum(r => r.TicketCount) ?? 0,
                Occurrences = evt.Occurrences?.GroupBy(o => o.OccurrenceId).Select(g => g.First()).Select(o => new EventOccurrenceDto
                {
                    OccurrenceId = o.OccurrenceId,
                    StartTime = o.StartTime,
                    EndTime = o.EndTime,
                    EventTitle = o.EventTitle,
                    IsCancelled = o.IsCancelled
                }).ToList(),
                Speakers = evt.Speakers?.GroupBy(s => s.SpeakerId).Select(g => g.First()).Select(s => new EventSpeakerDto
                {
                    Name = s.Name,
                    Bio = s.Bio,
                    PhotoUrl = s.PhotoUrl
                }).ToList(),
                Faqs = evt.Faqs?.GroupBy(f => f.FaqId).Select(g => g.First()).Select(f => new EventFaqDto
                {
                    Question = f.Question,
                    Answer = f.Answer
                }).ToList(),
                Media = evt.Media?.GroupBy(m => m.MediaId).Select(g => g.First()).Select(m => new EventMediaDto
                {
                    MediaType = m.MediaType.ToString(),
                    MediaUrl = m.MediaUrl,
                    Description = m.Description
                }).ToList(),
                IsVerifiedByAdmin = evt.IsVerifiedByAdmin,
                EditEventCount = evt.EditEventCount,
            };
        }
    }
    }
