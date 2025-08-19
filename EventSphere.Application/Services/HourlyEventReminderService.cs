using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

public class HourlyEventReminderService : BackgroundService
{
    // Public method to send a test reminder email for manual verification
    public async Task SendTestReminderEmailAsync()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var testEmail = config["Smtp:TestRecipient"] ?? "test@yourdomain.com";
            var testEventTitle = "Test Event";
            var testStartTime = DateTime.UtcNow.AddHours(1);
            var testBannerUrl = config["Smtp:BannerUrl"] ?? "https://eventsphere.com/default-banner.jpg";
            await SendReminderEmailAsync(testEmail, testEventTitle, testStartTime, config, testBannerUrl);
            Console.WriteLine($"[REMINDER] Test reminder email sent to {testEmail} for '{testEventTitle}' at {testStartTime}");
        }
    }
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1); // Run every minute for testing

    public HourlyEventReminderService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<backend.Data.AppDbContext>();
                    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                    var now = DateTime.UtcNow;
                    var oneHourLater = now.AddHours(1);

                    // Find event occurrences starting in exactly one hour
                    var occurrences = await dbContext.EventOccurrences
                        .Where(o => o.StartTime >= oneHourLater && o.StartTime < oneHourLater.AddMinutes(1))
                        .ToListAsync();

                    foreach (var occ in occurrences)
                    {
                        var eventEntity = await dbContext.Events.FirstOrDefaultAsync(e => e.EventId == occ.EventId);
                        var eventTitle = eventEntity?.Title ?? $"Event {occ.EventId}";
                        var bannerUrl = !string.IsNullOrWhiteSpace(eventEntity?.CoverImage) ? eventEntity.CoverImage : null;
                        var registrations = await dbContext.Registrations
                            .Where(r => r.EventId == occ.EventId)
                            .ToListAsync();

                        foreach (var reg in registrations)
                        {
                            if (!string.IsNullOrWhiteSpace(reg.UserEmail))
                            {
                                await SendReminderEmailAsync(reg.UserEmail, eventTitle, occ.StartTime, config, bannerUrl);
                                Console.WriteLine($"[REMINDER JOB] Sent reminder for occurrence {occ.EventId} to {reg.UserEmail} at {DateTime.UtcNow}");
                            }
                        }
                    }
                    Console.WriteLine($"[REMINDER JOB] Event reminder job executed at {DateTime.UtcNow}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in HourlyEventReminderService: {ex.Message}");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task SendReminderEmailAsync(string toEmail, string eventTitle, DateTime startTime, IConfiguration config)
    {
        await SendReminderEmailAsync(toEmail, eventTitle, startTime, config, null);

    }

    // Overload to accept bannerUrl
    private async Task SendReminderEmailAsync(string toEmail, string eventTitle, DateTime startTime, IConfiguration config, string bannerUrl)
    {
        var mail = new MailMessage();
        var smtpFrom = config["Smtp:From"] ?? "noreply@eventsphere.com";
        mail.From = new MailAddress(smtpFrom, "EventSphere");
        mail.To.Add(toEmail);
        mail.Subject = $"🎉 Hurry Up! '{eventTitle}' Starts Soon!";
        mail.IsBodyHtml = true;

        var safeBannerUrl = bannerUrl ?? config["Smtp:BannerUrl"] ?? "https://eventsphere.com/default-banner.jpg";

        mail.Body = $@"
            <div style='text-align:center;'>
                <img src='{safeBannerUrl}' alt='Event Banner' style='max-width:100%;height:auto;border-radius:8px;margin-bottom:20px;' />
                <h1 style='color:#2d8cf0;'>Don't Miss Out!</h1>
                <h2 style='color:#333;'>{eventTitle}</h2>
                <p style='font-size:1.2em;'>Your event starts at <b>{startTime.ToLocalTime():f}</b>.<br>
                🎉 Hurry up and join the excitement!<br>
                <p style='color:#888;'>See you there!</p>
            </div>
        ";

        try
        {
            using (var smtpClient = new SmtpClient())
            {
                smtpClient.Host = config["Smtp:Host"] ?? "smtp.example.com";
                smtpClient.Port = int.TryParse(config["Smtp:Port"], out var port) ? port : 587;
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new System.Net.NetworkCredential(
                    config["Smtp:User"] ?? "username",
                    config["Smtp:Pass"] ?? "password"
                );
                await smtpClient.SendMailAsync(mail);
                var now = DateTime.UtcNow;
                var timeSpan = startTime - now;
                Console.WriteLine($"[REMINDER] Sent reminder email to {toEmail} for '{eventTitle}' at {startTime} (TimeSpan: {timeSpan.TotalMinutes:F1} minutes from now)");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[REMINDER ERROR] Failed to send email to {toEmail}: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
