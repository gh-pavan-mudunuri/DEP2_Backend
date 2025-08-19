using System;
using System.Threading;
using System.Threading.Tasks;
using backend.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

public class HourlyEventCompletionService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(70);

    public HourlyEventCompletionService(IServiceProvider serviceProvider)
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
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var now = DateTime.UtcNow;

                    // Get events that ended before now and are not completed
                    var expiredEvents = await dbContext.Events
                        .Where(e => e.EventEnd < now && e.IsCompleted == 0)
                        .ToListAsync(stoppingToken);

                    if (expiredEvents.Any())
                    {
                        foreach (var ev in expiredEvents)
                        {
                            ev.IsCompleted = 1;
                        }

                        await dbContext.SaveChangesAsync(stoppingToken);
                        Console.WriteLine($"{expiredEvents.Count} events marked as completed at {now}.");
                    }
                    else
                    {
                        Console.WriteLine($"No events to mark as completed at {now}.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in HourlyEventCompletionService: {ex.Message}");
                // Optionally: log error to a file or DB
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
