using Microsoft.AspNetCore.Mvc;
using EventSphere.Domain.Entities;
using EventSphere.Application.Interfaces;
using EventSphere.Application.Dtos.Registrations;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EventSphere.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationsController : ControllerBase
    {
        private readonly IRegistrationService _registrationService;

        public RegistrationsController(IRegistrationService registrationService)
        {
            _registrationService = registrationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RegistrationDto>>> GetAll()
        {
            var registrations = await _registrationService.GetAllAsync();
            return Ok(registrations);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RegistrationDto>> GetById(int id)
        {
            var registration = await _registrationService.GetByIdAsync(id);
            if (registration == null)
                return NotFound();
            return Ok(registration);
        }

        [HttpPost]
        public async Task<ActionResult<RegistrationDto>> Create([FromBody] RegistrationRequestDto registrationDto)
        {
            if (registrationDto == null)
                return BadRequest("Invalid registration data.");

            var created = await _registrationService.CreateAsync(registrationDto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] RegistrationRequestDto registrationDto)
        {
            if (registrationDto == null)
                return BadRequest("Invalid registration data.");

            var updated = await _registrationService.UpdateAsync(id, registrationDto);
            if (!updated)
                return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _registrationService.DeleteAsync(id);
            if (!deleted)
                return NotFound();
            return NoContent();
        }
    }
}
