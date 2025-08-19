using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using EventSphere.Application.Interfaces;

public class ScheduledReminderService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1); // Check every 10 minutes

    public ScheduledReminderService(IServiceProvider serviceProvider)
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
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    await notificationService.ScheduleEventRemindersAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ScheduledReminderService: {ex.Message}");
            }
            await Task.Delay(_interval, stoppingToken);
        }
    }
}
