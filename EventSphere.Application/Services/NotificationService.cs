using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using EventSphere.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using backend.Data;

namespace EventSphere.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _dbContext;

        public NotificationService(IConfiguration configuration, AppDbContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            using (var smtpClient = new SmtpClient())
            {
                smtpClient.Host = _configuration["Smtp:Host"] ?? "smtp.example.com";
                smtpClient.Port = int.TryParse(_configuration["Smtp:Port"], out var port) ? port : 587;
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new System.Net.NetworkCredential(
                    _configuration["Smtp:User"] ?? "username",
                    _configuration["Smtp:Pass"] ?? "password"
                );
                var mail = new MailMessage();
                mail.From = new MailAddress(_configuration["Smtp:From"] ?? "noreply@eventsphere.com", "EventSphere");
                mail.To.Add(toEmail);
                mail.Subject = subject;
                mail.IsBodyHtml = true;
                mail.Body = body;
                await smtpClient.SendMailAsync(mail);
            }
        }

        // Schedules reminders for all upcoming events
        public async Task ScheduleEventRemindersAsync()
        {
            var now = DateTime.UtcNow;
            var events = await _dbContext.Events
                .Where(e => e.EventStart > now)
                .ToListAsync();
            foreach (var evt in events)
            {
                var registrations = await _dbContext.Registrations
                    .Where(r => r.EventId == evt.EventId)
                    .ToListAsync();
                foreach (var reg in registrations)
                {
                    // For each reminder time
                    await ScheduleReminderIfDue(evt, reg, -3);
                    await ScheduleReminderIfDue(evt, reg, -2);
                    await ScheduleReminderIfDue(evt, reg, -1);
                    await ScheduleReminderIfDue(evt, reg, 0, -1); // 1 hour before
                }
            }
        }

        private async Task ScheduleReminderIfDue(dynamic evt, dynamic reg, int daysBefore, int hoursBefore = 0)
        {
            var reminderTime = evt.EventStart.Date.AddDays(daysBefore).AddHours(hoursBefore);
            var now = DateTime.UtcNow;
            if (reminderTime > now && reminderTime < now.AddMinutes(60)) // Run if within next 10 min
            {
                var subject = $"Reminder: {evt.Title} is coming up soon!";
                var body = $"<p>Dear user,<br/>This is a reminder that your event <b>{evt.Title}</b> is scheduled for {evt.EventStart:yyyy-MM-dd HH:mm}.<br/>See you there!</p>";
                await SendEmailAsync(reg.UserEmail, subject, body);
            }
        }
    }
}
