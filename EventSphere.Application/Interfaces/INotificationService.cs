using System.Threading.Tasks;

namespace EventSphere.Application.Interfaces
{
	public interface INotificationService
	{
		Task SendEmailAsync(string toEmail, string subject, string body);
		Task ScheduleEventRemindersAsync();
	}
}
