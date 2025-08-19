using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EventSphere.API.Helpers;
using EventSphere.Application.Dtos.Events;

namespace EventSphere.API.Dtos
{
    public class UpdateEventApiDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? EventType { get; set; }
        public string? Location { get; set; }
        public DateTime? RegistrationDeadline { get; set; }
        public DateTime? EventStart { get; set; }
        public DateTime? EventEnd { get; set; }
        public bool? IsPaidEvent { get; set; }
        public decimal? Price { get; set; }
        public int? MaxAttendees { get; set; }
        public string? OrganizerName { get; set; }
        public string? OrganizerEmail { get; set; }
        public string? EventLink { get; set; }
        public string? RecurrenceType { get; set; }
        public string? RecurrenceRule { get; set; }
        public string? CustomFields { get; set; }
        [ModelBinder(BinderType = typeof(JsonModelBinder))]
        public List<OccurrenceDto>? Occurrences { get; set; }
        [ModelBinder(BinderType = typeof(JsonModelBinder))]
        public List<SpeakerDto>? Speakers { get; set; }
        [ModelBinder(BinderType = typeof(JsonModelBinder))]
        public List<FaqDto>? Faqs { get; set; }
        [ModelBinder(BinderType = typeof(JsonModelBinder))]
        public List<MediaDto>? Media { get; set; }
        public IFormFile? CoverImage { get; set; }
        public IFormFile? VibeVideo { get; set; }
    }
}
