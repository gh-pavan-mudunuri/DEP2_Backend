using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using EventSphere.Application.Dtos.Events;

namespace EventSphere.Application.Helpers
{
    public static class EventDtoHelper
    {
        public static void PopulateEventDtoFromForm(CreateEventDto dto, IFormCollection form)
        {
            var jsonSettings = new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Ignore };
            if (form.TryGetValue("Speakers", out var speakersVal) && speakersVal.Count > 0 && !string.IsNullOrWhiteSpace(speakersVal[0]))
            {
                var speakersJson = speakersVal[0];
                if (!string.IsNullOrWhiteSpace(speakersJson))
                    dto.Speakers = JsonConvert.DeserializeObject<List<SpeakerDto>>(speakersJson, jsonSettings);
            }
            if (form.TryGetValue("Faqs", out var faqsVal) && faqsVal.Count > 0 && !string.IsNullOrWhiteSpace(faqsVal[0]))
            {
                var faqsJson = faqsVal[0];
                if (!string.IsNullOrWhiteSpace(faqsJson))
                    dto.Faqs = JsonConvert.DeserializeObject<List<FaqDto>>(faqsJson, jsonSettings);
            }
            if (form.TryGetValue("Media", out var mediaVal) && mediaVal.Count > 0 && !string.IsNullOrWhiteSpace(mediaVal[0]))
            {
                var mediaJson = mediaVal[0];
                if (!string.IsNullOrWhiteSpace(mediaJson))
                    dto.Media = JsonConvert.DeserializeObject<List<MediaDto>>(mediaJson, jsonSettings);
            }
            // Occurrences deserialization
            if (form.TryGetValue("Occurrences", out var occurrencesVal) && occurrencesVal.Count > 0 && !string.IsNullOrWhiteSpace(occurrencesVal[0]))
            {
                var occurrencesJson = occurrencesVal[0];
                if (!string.IsNullOrWhiteSpace(occurrencesJson))
                    dto.Occurrences = JsonConvert.DeserializeObject<List<OccurrenceDto>>(occurrencesJson, jsonSettings);
            }
            // IsPaidEvent parsing
            if (form.TryGetValue("IsPaidEvent", out var paidVal) && paidVal.Count > 0)
            {
                var paidStr = paidVal[0]?.Trim().ToLower();
                dto.IsPaidEvent = paidStr == "true";
                System.Diagnostics.Debug.WriteLine($"[DEBUG] IsPaidEvent from form: '{paidStr}', parsed: {dto.IsPaidEvent}");
            }
            // Add more fields as needed
        }
    }
}
