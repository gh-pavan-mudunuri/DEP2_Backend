using Microsoft.AspNetCore.Mvc;
using EventSphere.Domain.Entities;
using EventSphere.Domain.Enums;
using backend.Services;
using backend.Dtos;
using backend.Interfaces;
using Microsoft.AspNetCore.Authorization;


namespace EventSphere.API.Controllers
{
    [ApiController]
    
    [Route("api/[controller]")]
    
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly backend.Interfaces.IEventService _eventService;
        private readonly IAdminService _adminService;
        private readonly EventSphere.Application.Repositories.IPaymentRepository _paymentRepository;

        public AdminController(IUserService userService, backend.Interfaces.IEventService eventService, IAdminService adminService, EventSphere.Application.Repositories.IPaymentRepository paymentRepository)
        {
            _userService = userService;
            _eventService = eventService;
            _adminService = adminService;
            _paymentRepository = paymentRepository;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("unapproved-events")]
        public async Task<IActionResult> GetUnapprovedEvents(int page = 1, int pageSize = 20)
        {
            var (events, totalCount) = await _eventService.GetUnapprovedEventsPagedAsync(page, pageSize);
            return Ok(new { events, totalCount });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto dto)
        {
            var result = await _userService.RegisterAsync(dto, UserRole.Admin);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("event/{id}/approve")]
        public async Task<IActionResult> ApproveEvent(int id)
        {
            var evt = await _eventService.GetEventByIdAsync(id);
            if (evt == null)
                return NotFound(new { success = false, message = "Event not found" });
            if (evt.IsVerifiedByAdmin)
                return BadRequest(new { success = false, message = "Event already approved" });
            evt.IsVerifiedByAdmin = true;
            evt.AdminVerifiedAt = DateTime.UtcNow;

            // Map only supported fields for UpdateEventDto
            var updateDto = new EventSphere.Application.Dtos.Events.UpdateEventDto
            {
                Title = evt.Title,
                Description = evt.Description,
                Category = evt.Category,
                EventType = evt.EventType.ToString(),
                Location = evt.Location,
                RegistrationDeadline = evt.RegistrationDeadline,
                EventStart = evt.EventStart,
                EventEnd = evt.EventEnd,
                IsVerifiedByAdmin = 1,
                IsPaidEvent = evt.IsPaidEvent,
                Price = evt.Price,
                MaxAttendees = evt.MaxAttendees,
                OrganizerName = evt.OrganizerName,
                OrganizerEmail = evt.OrganizerEmail,
                EventLink = evt.EventLink,
                RecurrenceType = evt.RecurrenceType,
                RecurrenceRule = evt.RecurrenceRule,
                CustomFields = evt.CustomFields
                // Add more fields here if your UpdateEventDto supports them
            };

            await _eventService.UpdateEventAsync(id, updateDto, null, false);
            return Ok(new { success = true, message = "Event approved" });
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("event-edit/{id}/approve")]
        public async Task<IActionResult> AproveEventEdit(int id)
        {
            var evt = await _eventService.GetEventByIdAsync(id);
            if (evt == null)
                return NotFound(new { success = false, message = "Event not found" });
            if (evt.EditEventCount >= 0)
                return BadRequest(new { success = false, message = "No pending edits to approve" });
            await _adminService.AproveEventEditAsync(id);
            return Ok(new { success = true, message = "Event edit approved" });
        }

        [HttpGet("event/{eventId}/payments")]
        public async Task<IActionResult> GetPaymentsForEvent(int eventId)
        {
            var payments = await _paymentRepository.GetPaymentsForEventAsync(eventId);
            return Ok(payments);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("payments")]
        public async Task<IActionResult> GetAllEventPayments()
        {
            var payments = await _paymentRepository.GetAllEventPaymentsAsync();
            return Ok(payments);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("events")]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _eventService.GetAllEventsAsync();
            return Ok(events);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("event-income-summary")]
        public async Task<IActionResult> GetEventIncomeSummary()
        {
            var summary = await _paymentRepository.GetEventIncomeSummaryAsync();
            return Ok(summary);
        }

    }
}
