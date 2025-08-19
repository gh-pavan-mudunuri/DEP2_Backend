using Microsoft.AspNetCore.Mvc;
using EventSphere.Domain.Entities;

namespace EventSphere.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        // TODO: Inject your ticket service here (ITicketService) and use it for data access

        [HttpGet]
        public IActionResult GetAll()
        {
            // TODO: Return all tickets
            return Ok();
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            // TODO: Return ticket by id
            return Ok();
        }

        [HttpPost]
        public IActionResult Create([FromBody] Ticket ticket)
        {
            // TODO: Create new ticket
            return Ok();
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Ticket ticket)
        {
            // TODO: Update ticket
            return Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            // TODO: Delete ticket
            return Ok();
        }
    }
}
