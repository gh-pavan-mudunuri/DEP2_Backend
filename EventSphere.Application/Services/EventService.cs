using EventSphere.Domain.Enums;
using backend.Interfaces;
using EventSphere.Domain.Entities;
using EventSphere.Application.Dtos;
using EventSphere.Application.Dtos.Events;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventSphere.Application.Repositories;
using EventSphere.Application.Helpers;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace backend.Services
{
    public class EventService : IEventService
    {
        public async Task<(IEnumerable<EventCardDto> Events, int TotalCount)> GetUnapprovedEventsPagedAsync(int page, int pageSize)
        {
            return await _eventRepository.GetUnapprovedEventsPagedAsync(page, pageSize);
        }

        /// <summary>
        /// Handles all file extraction, media file saving, and description placeholder replacement logic for event creation.
        /// </summary>
        public async Task<Event> MapAndCreateEventWithFilesAsync(CreateEventDto dto, int organizerId, HttpRequest request)
        {
            // Populate DTO from form if present
            var form = request.HasFormContentType ? await request.ReadFormAsync() : null;
            if (form != null)
            {
                EventSphere.Application.Helpers.EventDtoHelper.PopulateEventDtoFromForm(dto, form);
                // Debug log for Occurrences
                if (dto.Occurrences != null)
                {
                    _logger.LogInformation($"[DEBUG] Occurrences after deserialization: {System.Text.Json.JsonSerializer.Serialize(dto.Occurrences)}");
                }
                else
                {
                    _logger.LogInformation("[DEBUG] Occurrences after deserialization: null");
                }
            }

            // Extract files from the request
            var mediaFiles = new List<IFormFile>();
            var speakerPhotos = new List<IFormFile>();
            if (request.HasFormContentType && request.Form.Files.Count > 0)
            {
                foreach (var file in request.Form.Files)
                {
                    if (file.Name == "media")
                        mediaFiles.Add(file);
                    else if (file.Name.StartsWith("speakers["))
                        speakerPhotos.Add(file);
                }
            }
            _logger.LogInformation($"[DEBUG] Number of media files received: {mediaFiles.Count}");
            _logger.LogInformation($"[DEBUG] Number of speaker photos received: {speakerPhotos.Count}");
            // Save media files and replace placeholders in description
            if (mediaFiles.Count > 0 && !string.IsNullOrEmpty(dto.Description))
            {
                if (dto.Media == null) dto.Media = new List<MediaDto>();
                for (int i = 0; i < mediaFiles.Count; i++)
                {
                    var ext = System.IO.Path.GetExtension(mediaFiles[i].FileName).ToLower();
                    var isVideo = ext == ".mp4" || ext == ".webm" || ext == ".mov" || ext == ".avi" || ext == ".mkv";
                    _logger.LogInformation($"[DEBUG] Media file {i}: Name={mediaFiles[i].FileName}, Ext={ext}, IsVideo={isVideo}");
                    var url = await FileHelper.SaveFileAsync(mediaFiles[i], "media");
                    _logger.LogInformation($"[DEBUG] Saved media file {i} to: {url}");
                    dto.Description = dto.Description.Replace($"__MEDIA_{i}__", url);
                    dto.Media.Add(new MediaDto
                    {
                        MediaUrl = url,
                        MediaType = isVideo ? "Video" : "Image",
                        Description = (isVideo ? "Video" : "Image") + " from event description",
                        MediaFile = mediaFiles[i]
                    });
                }
            }
            // Use cover image and vibe video from DTO if present
            var evt = await MapAndCreateEventAsync(dto, organizerId, dto.CoverImage, dto.VibeVideo, dto.Media, speakerPhotos, request);
            return evt;
        }
        public async Task<Event> MapAndCreateEventAsync(
            CreateEventDto dto,
            int organizerId,
            IFormFile? coverImage,
            IFormFile? vibeVideo,
            List<MediaDto>? mediaDtos,
            List<IFormFile>? speakerPhotos,
            HttpRequest? request)
        {
            // Map CreateEventDto to Event entity

            // Extract video URLs from the description
            var videoUrls = new List<string>();
            if (dto.Description != null)
            {
                // Extract base64 video data from <video src="data:video/mp4;base64,...">
                var base64VideoMatches = System.Text.RegularExpressions.Regex.Matches(
                    dto.Description ?? string.Empty,
                    "<video[^>]*src=\\\"data:video/(?<ext>mp4|webm|mov|avi|mkv);base64,(?<data>[A-Za-z0-9+/=]+)\\\"[^>]*>",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                int base64Index = 1;
                foreach (System.Text.RegularExpressions.Match match in base64VideoMatches)
                {
                    var ext = match.Groups["ext"].Value;
                    var base64Data = match.Groups["data"].Value;
                    if (!string.IsNullOrEmpty(base64Data) && !string.IsNullOrEmpty(ext))
                    {
                        try
                        {
                            var bytes = Convert.FromBase64String(base64Data);
                            var folder = System.IO.Path.Combine("wwwroot", "uploads", "media-videos");
                            if (!System.IO.Directory.Exists(folder))
                                System.IO.Directory.CreateDirectory(folder);
                            var fileName = $"desc_video_{DateTime.UtcNow:yyyyMMddHHmmssfff}_{base64Index}.{ext}";
                            var filePath = System.IO.Path.Combine(folder, fileName);
                            System.IO.File.WriteAllBytes(filePath, bytes);
                            var localPath = "/uploads/media-videos/" + fileName;
                            videoUrls.Add(localPath); // Add the local path to videoUrls for further processing
                            _logger.LogInformation($"[Base64Video] Saved base64 video to: {localPath}");
                            base64Index++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "[Base64Video] Failed to save base64 video from description.");
                        }
                    }
                }

                // Simple regex for video URLs (mp4, webm, mov, avi, mkv)
                var matches = System.Text.RegularExpressions.Regex.Matches(
                    dto.Description ?? string.Empty,
                    @"https?://[^\s'""]+\.(mp4|webm|mov|avi|mkv)",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    if (!string.IsNullOrEmpty(match.Value))
                        videoUrls.Add(match.Value);
                }
                _logger.LogInformation($"[VideoExtract] Extracted video URLs: {string.Join(", ", videoUrls)}");
            }
            {
                // Simple regex for video URLs (mp4, webm, mov, avi, mkv)
                var matches = System.Text.RegularExpressions.Regex.Matches(
                    dto.Description,
                    @"https?://[^\s'\""]+\.(mp4|webm|mov|avi|mkv)",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    if (!string.IsNullOrEmpty(match.Value))
                        videoUrls.Add(match.Value);
                }
                _logger.LogInformation($"[VideoExtract] Extracted video URLs: {string.Join(", ", videoUrls)}");
            }

            // Build media list from mediaDtos (from frontend) and extracted videos
            var mediaList = new List<EventMedia>();
            if (mediaDtos != null)
            {
                mediaList.AddRange(mediaDtos.Select(m => new EventMedia
                {
                    MediaType = MapMediaTypeWithDebug(m.MediaType, m.MediaUrl),
                    MediaUrl = m.MediaUrl,
                    Description = m.Description
                }));
            }
            // Add extracted video URLs as EventMedia (for redundancy, in case frontend missed any)
            foreach (var url in videoUrls)
            {
                if (!mediaList.Any(m => m.MediaUrl == url))
                {
                    _logger.LogInformation($"[VideoDownload] Attempting to process video URL: {url}");
                    string? localPath = null;
                    try
                    {
                        using (var httpClient = new System.Net.Http.HttpClient())
                        {
                            _logger.LogInformation($"[VideoDownload] Sending HTTP GET for: {url}");
                            var response = await httpClient.GetAsync(url);
                            _logger.LogInformation($"[VideoDownload] HTTP status for {url}: {response.StatusCode}");
                            if (response.IsSuccessStatusCode)
                            {
                                var bytes = await response.Content.ReadAsByteArrayAsync();
                                var fileName = System.IO.Path.GetFileName(new Uri(url).LocalPath);
                                var folder = System.IO.Path.Combine("wwwroot", "uploads", "media-videos");
                                _logger.LogInformation($"[VideoDownload] Saving to folder: {folder}");
                                if (!System.IO.Directory.Exists(folder))
                                {
                                    System.IO.Directory.CreateDirectory(folder);
                                    _logger.LogInformation($"[VideoDownload] Created directory: {folder}");
                                }
                                var filePath = System.IO.Path.Combine(folder, fileName);
                                await System.IO.File.WriteAllBytesAsync(filePath, bytes);
                                _logger.LogInformation($"[VideoDownload] Saved file: {filePath}");
                                // Store the relative path for serving
                                localPath = "/uploads/media-videos/" + fileName;
                            }
                            else
                            {
                                _logger.LogWarning($"[VideoDownload] Failed to download video. Status: {response.StatusCode} for URL: {url}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"[VideoDownload] Exception while downloading video from URL: {url}");
                    }
                    if (localPath != null)
                        _logger.LogInformation($"[VideoDownload] Video saved and will be stored in DB as: {localPath}");
                    else
                        _logger.LogWarning($"[VideoDownload] Video not saved locally, storing original URL in DB: {url}");
                    mediaList.Add(new EventMedia
                    {
                        MediaType = MediaType.Video,
                        MediaUrl = localPath ?? url,
                        Description = "Extracted from description"
                    });
                }
            }

            // Generate occurrences based on recurrence type
            List<EventOccurrence> occurrences = new List<EventOccurrence>();
            string mainTitle = dto.Title ?? string.Empty;
            // If this is an update and dto.Title is null or empty, fallback to existing.Title
            // (This block should be enabled in update context only)
            // if (string.IsNullOrWhiteSpace(mainTitle) && existing != null)
            //     mainTitle = existing.Title ?? string.Empty;
            // Rule-based recurrence: store main event time as first occurrence, then generate next 10 occurrences from now
            if (!string.IsNullOrEmpty(dto.RecurrenceType) && dto.RecurrenceType.ToLower() == "rule" && !string.IsNullOrEmpty(dto.RecurrenceRule) && dto.EventStart.HasValue && dto.EventEnd.HasValue)
            {
                // Always add the main event's start/end as the first occurrence
                occurrences.Add(new EventOccurrence
                {
                    StartTime = dto.EventStart.Value,
                    EndTime = dto.EventEnd.Value,
                    EventTitle = mainTitle
                });
                var rule = dto.RecurrenceRule.ToUpper();
                string freq = "DAILY";
                foreach (var part in rule.Split(';'))
                {
                    if (part.StartsWith("FREQ=")) freq = part.Substring(5);
                }
                var duration = dto.EventEnd.Value - dto.EventStart.Value;
                var start = dto.EventStart.Value;
                for (int i = 1; i <= 10; i++) // start from 1 to avoid duplicating the main event occurrence
                {
                    if (freq == "DAILY")
                    {
                        occurrences.Add(new EventOccurrence
                        {
                            StartTime = start.AddDays(i),
                            EndTime = start.AddDays(i).Add(duration),
                            EventTitle = mainTitle
                        });
                    }
                    else if (freq == "WEEKLY")
                    {
                        occurrences.Add(new EventOccurrence
                        {
                            StartTime = start.AddDays(i * 7),
                            EndTime = start.AddDays(i * 7).Add(duration),
                            EventTitle = mainTitle
                        });
                    }
                    // Add more FREQ support as needed
                }
            }
            // Custom recurrence: store main event time as first occurrence, then all user-entered custom dates
            else if (!string.IsNullOrEmpty(dto.RecurrenceType) && dto.RecurrenceType.ToLower() == "custom")
            {
                // Only add main event time if not already present in custom occurrences
                bool mainInCustom = false;
                if (dto.Occurrences != null && dto.Occurrences.Count > 0)
                {
                    _logger.LogInformation("[CustomOccurrences] Received occurrences: {0}", System.Text.Json.JsonSerializer.Serialize(dto.Occurrences));
                    foreach (var o in dto.Occurrences)
                    {
                        // Defensive: If StartTime/EndTime are not set, try to parse from string (if available)
                        DateTime? start = o.StartTime;
                        DateTime? end = o.EndTime;
                        // If StartTime or EndTime is not set, log and skip
                        if (!start.HasValue || !end.HasValue)
                        {
                            _logger.LogWarning($"[CustomOccurrences] Occurrence missing StartTime or EndTime. Skipping. Raw: {System.Text.Json.JsonSerializer.Serialize(o)}");
                            continue;
                        }
                        // If only time is present (date is today), log a warning
                        if (start.Value.Date == DateTime.UtcNow.Date || end.Value.Date == DateTime.UtcNow.Date)
                        {
                            _logger.LogWarning($"[CustomOccurrences] Occurrence StartTime or EndTime has today's date. Possible frontend bug. Start: {start}, End: {end}");
                        }
                        if (start.Value == end.Value)
                        {
                            _logger.LogWarning($"[CustomOccurrences] Skipping occurrence with identical StartTime and EndTime: {start.Value}");
                            continue;
                        }
                        // Check if this matches the main event time
                        if (dto.EventStart.HasValue && dto.EventEnd.HasValue &&
                            start.Value == dto.EventStart.Value &&
                            end.Value == dto.EventEnd.Value)
                        {
                            mainInCustom = true;
                        }
                        occurrences.Add(new EventOccurrence
                        {
                            StartTime = start.Value,
                            EndTime = end.Value,
                            EventTitle = o.EventTitle ?? mainTitle
                        });
                    }
                }
                // Only add main event time if not already present in custom occurrences and both are provided
                if (!mainInCustom && dto.EventStart.HasValue && dto.EventEnd.HasValue)
                {
                    occurrences.Insert(0, new EventOccurrence
                    {
                        StartTime = dto.EventStart.Value,
                        EndTime = dto.EventEnd.Value,
                        EventTitle = mainTitle
                    });
                }
            }
            // No recurrence: just add one occurrence for the event
            else if (dto.EventStart.HasValue && dto.EventEnd.HasValue)
            {
                occurrences.Add(new EventOccurrence
                {
                    StartTime = dto.EventStart.Value,
                    EndTime = dto.EventEnd.Value,
                    EventTitle = mainTitle
                });
            }

            var isRecurring = false;
            if (!string.IsNullOrEmpty(dto.RecurrenceType))
            {
                var type = dto.RecurrenceType.ToLower();
                if (type == "custom" || type == "rule")
                    isRecurring = true;
            }
            if (occurrences.Count > 1)
                isRecurring = true;

            string? customFields = null;
            if ((dto.Category?.ToLower() == "other") && !string.IsNullOrWhiteSpace(dto.OtherCategory))
            {
                customFields = System.Text.Json.JsonSerializer.Serialize(new { OtherCategory = dto.OtherCategory });
            }

            var evt = new Event
            {
                Title = dto.Title ?? string.Empty,
                Description = dto.Description ?? string.Empty,
                Category = dto.Category ?? string.Empty,
                // EventType: only map if property exists
                // EventType = dto.EventType, // Uncomment if exists
                EventType = !string.IsNullOrEmpty(dto.EventType) && Enum.TryParse<EventSphere.Domain.Enums.EventType>(dto.EventType, true, out var parsedType) ? parsedType : EventSphere.Domain.Enums.EventType.TBA,
                Location = dto.Location ?? string.Empty,
                RegistrationDeadline = dto.RegistrationDeadline.HasValue ? dto.RegistrationDeadline.Value : throw new ArgumentException("RegistrationDeadline is required"),
                EventStart = dto.EventStart.HasValue ? dto.EventStart.Value : throw new ArgumentException("EventStart is required"),
                EventEnd = dto.EventEnd.HasValue ? dto.EventEnd.Value : throw new ArgumentException("EventEnd is required"),
                Price = dto.Price,
                OrganizerId = organizerId,
                OrganizerName = dto.OrganizerName ?? string.Empty, // fallback if OrganizerName not present in DTO
                OrganizerEmail = dto.OrganizerEmail,
                RecurrenceType = dto.RecurrenceType,
                RecurrenceRule = dto.RecurrenceRule,
                MaxAttendees = dto.MaxAttendees,
                EventLink = dto.EventLink,
                Media = mediaList,
                Faqs = dto.Faqs?.Select(f => new EventFaq { Question = f.Question, Answer = f.Answer }).ToList(),
                Occurrences = occurrences,
                Speakers = dto.Speakers?.Select(s => new EventSpeaker { Name = s.Name, Bio = s.Bio, PhotoUrl = s.PhotoUrl, SocialLinks = s.SocialLinks }).ToList(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsRecurring = isRecurring,
                CustomFields = customFields,
                IsPaidEvent = dto.IsPaidEvent
            };

            // Use existing CreateEventAsync to handle file uploads and DB insert
            var createdEvent = await CreateEventAsync(evt, coverImage, vibeVideo, mediaDtos, speakerPhotos);
            return createdEvent;
        }

        private readonly IEventRepository _eventRepository;
        private readonly ILogger<EventService> _logger;
        private readonly EventSphere.Application.Interfaces.INotificationService _notificationService;

        public EventService(IEventRepository eventRepository, ILogger<EventService> logger, EventSphere.Application.Interfaces.INotificationService notificationService)
        {
            _eventRepository = eventRepository;
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            return await _eventRepository.GetAllEventsAsync();
        }

        public async Task<(IEnumerable<Event> Events, int TotalCount)> GetEventsPagedAsync(int page, int pageSize)
        {
            return await _eventRepository.GetEventsPagedAsync(page, pageSize);
        }

        public async Task<EventDto?> GetEventByIdNewAsync(int id)
        {
            return await _eventRepository.GetEventByIdNewAsync(id);
        }

        public async Task<Event?> GetEventByIdAsync(int id)
        {
            return await _eventRepository.GetEventByIdAsync(id);
        }

        public async Task<Event?> UpdateEventAsync(int id, UpdateEventDto dto, List<IFormFile>? speakerPhotos = null, bool decrementEditCount = true)
        {
            // Use GetEventByIdNewAsync (returns EventDto)
            var eventDto = await _eventRepository.GetEventByIdNewAsync(id);
            if (eventDto == null)
                return null;

            // Convert EventDto to Event entity for update
            var eventEntity = new Event
            {
                EventId = eventDto.EventId,
                Title = dto.Title ?? eventDto.Title ?? string.Empty,
                Description = dto.Description ?? eventDto.Description ?? string.Empty,
                Category = dto.Category ?? eventDto.Category ?? string.Empty,
                EventType = !string.IsNullOrEmpty(dto.EventType) && Enum.TryParse<EventSphere.Domain.Enums.EventType>(dto.EventType, true, out var parsedType) ? parsedType : (Enum.TryParse<EventSphere.Domain.Enums.EventType>(eventDto.EventType, true, out var fallbackType) ? fallbackType : EventSphere.Domain.Enums.EventType.TBA),
                Location = dto.Location ?? eventDto.Location ?? string.Empty,
                RegistrationDeadline = dto.RegistrationDeadline ?? eventDto.RegistrationDeadline,
                EventStart = dto.EventStart ?? eventDto.EventStart,
                EventEnd = dto.EventEnd ?? eventDto.EventEnd,
                IsPaidEvent = dto.IsPaidEvent ?? eventDto.IsPaidEvent,
                Price = dto.Price ?? eventDto.Price,
                MaxAttendees = dto.MaxAttendees ?? eventDto.MaxAttendees,
                OrganizerName = dto.OrganizerName ?? eventDto.OrganizerName ?? string.Empty,
                OrganizerEmail = dto.OrganizerEmail ?? eventDto.OrganizerEmail ?? string.Empty,
                EventLink = dto.EventLink ?? eventDto.EventLink,
                RecurrenceType = dto.RecurrenceType ?? eventDto.RecurrenceType,
                RecurrenceRule = dto.RecurrenceRule ?? eventDto.RecurrenceRule,
                CustomFields = !string.IsNullOrWhiteSpace(dto.CustomFields) ? dto.CustomFields : eventDto.CustomFields,
                UpdatedAt = DateTime.UtcNow,
                // CreatedAt = eventDto.CreatedAt, // Remove if not present in DTO
                EditEventCount = eventDto.EditEventCount,
                IsVerifiedByAdmin = eventDto.IsVerifiedByAdmin,
                OrganizerId = eventDto.OrganizerId,
                CoverImage = eventDto.CoverImage,
                VibeVideoUrl = eventDto.VibeVideoUrl,
                IsRecurring = eventDto.IsRecurring,
                // Media, Faqs, Occurrences, Speakers will be handled below
            };

            // Edit count logic
            if (decrementEditCount && eventDto.EditEventCount < 0)
                throw new InvalidOperationException("Edit limit exceeded, Last Edit is not approved by Admin Yet.");
            if (eventDto.EditEventCount == 0 && decrementEditCount)
            {
                eventEntity.CustomFields = System.Text.Json.JsonSerializer.Serialize(dto);
                eventEntity.EditEventCount = -1;
                eventEntity.IsVerifiedByAdmin = false;
            }
            else
            {
                eventEntity.EditEventCount = decrementEditCount ? eventDto.EditEventCount - 1 : eventDto.EditEventCount;
            }

            // Handle file uploads for edit
            if (dto.CoverImage != null)
            {
                eventEntity.CoverImage = await FileHelper.SaveFileAsync(dto.CoverImage, "covers");
            }
            if (dto.VibeVideo != null)
            {
                eventEntity.VibeVideoUrl = await FileHelper.SaveFileAsync(dto.VibeVideo, "videos");
            }

            // Occurrences
            if (!string.IsNullOrEmpty(eventEntity.RecurrenceType) && eventEntity.RecurrenceType.ToLower() == "rule" && !string.IsNullOrEmpty(eventEntity.RecurrenceRule))
            {
                var occurrences = new List<EventOccurrence>();
                var mainTitle = eventEntity.Title ?? string.Empty;
                occurrences.Add(new EventOccurrence
                {
                    StartTime = eventEntity.EventStart,
                    EndTime = eventEntity.EventEnd,
                    EventTitle = mainTitle,
                    EventId = eventEntity.EventId
                });
                var rule = eventEntity.RecurrenceRule.ToUpper();
                string freq = "DAILY";
                foreach (var part in rule.Split(';'))
                {
                    if (part.StartsWith("FREQ=")) freq = part.Substring(5);
                }
                var duration = eventEntity.EventEnd - eventEntity.EventStart;
                var start = eventEntity.EventStart;
                for (int i = 1; i < 10; i++)
                {
                    if (freq == "DAILY")
                    {
                        occurrences.Add(new EventOccurrence
                        {
                            StartTime = start.AddDays(i),
                            EndTime = start.AddDays(i).Add(duration),
                            EventTitle = mainTitle,
                            EventId = eventEntity.EventId
                        });
                    }
                    else if (freq == "WEEKLY")
                    {
                        occurrences.Add(new EventOccurrence
                        {
                            StartTime = start.AddDays(i * 7),
                            EndTime = start.AddDays(i * 7).Add(duration),
                            EventTitle = mainTitle,
                            EventId = eventEntity.EventId
                        });
                    }
                }
                eventEntity.Occurrences = occurrences;
            }
            else if (dto.Occurrences != null)
            {
                eventEntity.Occurrences = dto.Occurrences.Select(o => new EventOccurrence
                {
                    StartTime = ((System.Nullable<System.DateTime>)o.StartTime).HasValue ? ((System.Nullable<System.DateTime>)o.StartTime).Value : DateTime.UtcNow,
                    EndTime = ((System.Nullable<System.DateTime>)o.EndTime).HasValue ? ((System.Nullable<System.DateTime>)o.EndTime).Value : DateTime.UtcNow,
                    EventTitle = o.EventTitle,
                    EventId = eventEntity.EventId
                }).ToList();
            }
            else if (eventDto.Occurrences != null)
            {
                eventEntity.Occurrences = eventDto.Occurrences.Select(o => new EventOccurrence
                {
                    StartTime = ((System.Nullable<System.DateTime>)o.StartTime).HasValue ? ((System.Nullable<System.DateTime>)o.StartTime).Value : DateTime.UtcNow,
                    EndTime = ((System.Nullable<System.DateTime>)o.EndTime).HasValue ? ((System.Nullable<System.DateTime>)o.EndTime).Value : DateTime.UtcNow,
                    EventTitle = o.EventTitle,
                    EventId = eventEntity.EventId
                }).ToList();
            }

            // Speakers
            if (dto.Speakers != null)
            {
                eventEntity.Speakers = new List<EventSpeaker>();
                var speakerPhotoDict = new Dictionary<int, IFormFile>();
                if (speakerPhotos != null)
                {
                    foreach (var file in speakerPhotos)
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(file.Name, @"speakers\[(\d+)\]\\?.*image");
                        if (match.Success && int.TryParse(match.Groups[1].Value, out int idx))
                        {
                            speakerPhotoDict[idx] = file;
                        }
                    }
                }
                for (int i = 0; i < dto.Speakers.Count; i++)
                {
                    var s = dto.Speakers[i];
                    string photoUrl = !string.IsNullOrEmpty(s.PhotoUrl) ? s.PhotoUrl : "/uploads/speakers/default-profile.png";
                    if (speakerPhotoDict.TryGetValue(i, out var file) && file != null)
                    {
                        var fileName = await FileHelper.SaveFileAsync(file, "speakers");
                        photoUrl = "/uploads/speakers/" + System.IO.Path.GetFileName(fileName);
                    }
                    eventEntity.Speakers.Add(new EventSpeaker
                    {
                        Name = s.Name,
                        Bio = s.Bio,
                        PhotoUrl = photoUrl,
                        EventId = eventEntity.EventId,
                        EventTitle = eventEntity.Title
                    });
                }
            }
            else
            {
                eventEntity.Speakers = eventDto.Speakers?.Select(s => new EventSpeaker
                {
                    Name = s.Name,
                    Bio = s.Bio,
                    PhotoUrl = s.PhotoUrl,
                    EventId = eventEntity.EventId,
                    EventTitle = eventEntity.Title
                }).ToList();
            }

            // Faqs
            if (dto.Faqs != null)
            {
                eventEntity.Faqs = dto.Faqs.Select(f => new EventFaq
                {
                    Question = f.Question,
                    Answer = f.Answer,
                    EventId = eventEntity.EventId,
                    EventTitle = eventEntity.Title
                }).ToList();
            }
            else
            {
                eventEntity.Faqs = eventDto.Faqs?.Select(f => new EventFaq
                {
                    Question = f.Question,
                    Answer = f.Answer,
                    EventId = eventEntity.EventId,
                    EventTitle = eventEntity.Title
                }).ToList();
            }

            // Media
            eventEntity.Media = eventDto.Media?.Select(m => new EventMedia
            {
                MediaId = m.MediaId,
                MediaType = MapMediaTypeWithDebug(m.MediaType, m.MediaUrl),
                MediaUrl = m.MediaUrl,
                Description = m.Description,
                EventId = eventEntity.EventId,
                EventTitle = eventEntity.Title
            }).ToList();

            await _eventRepository.UpdateEventAsync(eventEntity);

            var registrations = await _eventRepository.GetRegistrationsByEventIdAsync(eventEntity.EventId);
            foreach (var reg in registrations)
            {
                if (!string.IsNullOrWhiteSpace(reg.UserEmail))
                {
                    var subject = $"Event Updated: {eventEntity.Title}";
                    var body = $"Dear user,<br/>The event <b>{eventEntity.Title}</b> you registered for has been updated. Please check the event details for changes.";
                    await _notificationService.SendEmailAsync(reg.UserEmail, subject, body);
                }
            }

            return eventEntity;
        }

        public async Task<bool> DeleteEventAsync(int id)
        {
            var result = await _eventRepository.DeleteEventAsync(id);
            return result;
        }


        public async Task<Event> CreateEventAsync(
            Event evt,
            IFormFile? coverImage,
            IFormFile? vibeVideo,
            List<MediaDto>? mediaDtos,
            List<IFormFile>? speakerPhotos)
        {
            try
            {
                _logger.LogInformation("Creating event: {title}", evt.Title);

                if (coverImage != null)
                    evt.CoverImage = await FileHelper.SaveFileAsync(coverImage, "covers");

                if (vibeVideo != null)
                    evt.VibeVideoUrl = await FileHelper.SaveFileAsync(vibeVideo, "videos");

                if (evt.Faqs?.Count > 0)
                {
                    foreach (var faq in evt.Faqs)
                    {
                        faq.FaqId = 0;
                        faq.EventTitle = evt.Title;
                    }
                }

                if (evt.Media?.Count > 0)
                {
                    var mediaList = evt.Media.ToList();
                    for (int i = 0; i < mediaList.Count; i++)
                    {
                        var media = mediaList[i];
                        media.MediaId = 0;
                        media.Event = evt;
                        media.EventTitle = evt.Title;
                        media.Description = evt.Description;
                        // Ensure EventId is set if needed (for detached entities)
                        // media.EventId = evt.EventId; // Uncomment if required by your model

                        if (mediaDtos != null && i < mediaDtos.Count && mediaDtos[i].MediaFile != null)
                        {
                            var file = mediaDtos[i].MediaFile!;
                            var folder = media.MediaType == MediaType.Image ? "media-images" : "media-videos";
                            media.MediaUrl = await FileHelper.SaveFileAsync(file, folder);
                        }
                    }
                }

                if (evt.Occurrences?.Count > 0)
                {
                    if (evt.Occurrences is not List<EventOccurrence>)
                        evt.Occurrences = evt.Occurrences.ToList();

                    foreach (var occ in evt.Occurrences)
                    {
                        occ.OccurrenceId = 0;
                    }
                }

                if (evt.Speakers?.Count > 0)
                {
                    var speakersList = evt.Speakers.ToList();
                    for (int i = 0; i < speakersList.Count; i++)
                    {
                        var speaker = speakersList[i];
                        speaker.SpeakerId = 0;
                        speaker.EventTitle = evt.Title;

                        if (speakerPhotos != null && i < speakerPhotos.Count && speakerPhotos[i] != null)
                        {
                            var fileName = await FileHelper.SaveFileAsync(speakerPhotos[i], "speakers");
                            speaker.PhotoUrl = "/uploads/speakers/" + System.IO.Path.GetFileName(fileName);
                        }
                        else if (string.IsNullOrEmpty(speaker.PhotoUrl))
                        {
                            speaker.PhotoUrl = "/uploads/speakers/default-profile.png";
                        }
                    }
                }

                await _eventRepository.AddEventAsync(evt);

                _logger.LogInformation("Event created successfully: {title}", evt.Title);
                return evt;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event: {title}", evt.Title);
                throw;
            }
        }

        private MediaType MapMediaTypeWithDebug(string? mediaTypeStr, string? mediaUrl)
        {
            var original = mediaTypeStr;
            if (string.IsNullOrWhiteSpace(mediaTypeStr))
            {
                if (!string.IsNullOrEmpty(mediaUrl) &&
                    (mediaUrl.EndsWith(".mp4") || mediaUrl.EndsWith(".webm") || mediaUrl.EndsWith(".mov") ||
                     mediaUrl.EndsWith(".avi") || mediaUrl.EndsWith(".mkv")))
                {
                    _logger.LogWarning($"[MediaType Fallback] No MediaType string, guessed Video from url: {mediaUrl}");
                    return MediaType.Video;
                }

                _logger.LogWarning($"[MediaType Fallback] No MediaType string, defaulting to Image. Url: {mediaUrl}");
                return MediaType.Image;
            }

            var trimmed = mediaTypeStr.Trim();
            if (Enum.TryParse<MediaType>(trimmed, true, out var mt))
            {
                _logger.LogInformation($"[MediaType Parse] Parsed '{original}' as {mt}");
                return mt;
            }

            if (!string.IsNullOrEmpty(mediaUrl) &&
                (mediaUrl.EndsWith(".mp4") || mediaUrl.EndsWith(".webm") || mediaUrl.EndsWith(".mov") ||
                 mediaUrl.EndsWith(".avi") || mediaUrl.EndsWith(".mkv")))
            {
                _logger.LogWarning($"[MediaType Fallback] Could not parse '{original}', guessed Video from url: {mediaUrl}");
                return MediaType.Video;
            }

            _logger.LogWarning($"[MediaType Fallback] Could not parse '{original}', defaulting to Image. Url: {mediaUrl}");
            return MediaType.Image;
        }

        public async Task<int?> GetEventEditCountAsync(int eventId)
        {
            return await _eventRepository.GetEventEditCountAsync(eventId);
        }

        public async Task<int> GetRegistrationCountForEventAsync(int eventId)
        {
            return await _eventRepository.GetRegistrationCountForEventAsync(eventId);
        }


    }
}
