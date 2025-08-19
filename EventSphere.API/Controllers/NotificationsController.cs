using Microsoft.AspNetCore.Mvc;
using EventSphere.Domain.Entities;

namespace EventSphere.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        // TODO: Inject your notification service here (INotificationService) and use it for data access

        [HttpGet]
        public IActionResult GetAll()
        {
            // TODO: Return all notifications
            return Ok();
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            // TODO: Return notification by id
            return Ok();
        }

        [HttpPost]
        public IActionResult Create([FromBody] Notification notification)
        {
            // TODO: Create new notification
            return Ok();
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Notification notification)
        {
            // TODO: Update notification
            return Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            // TODO: Delete notification
            return Ok();
        }
    }
}
