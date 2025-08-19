using backend.Dtos;
using backend.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/email")]
    public class EmailController : ControllerBase
    {
        private readonly SmtpEmailSender _emailSender;
        public EmailController(SmtpEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        [HttpPost("send-to-organiser")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendToOrganiser([FromBody] SendEmailDto dto)
        {
            await _emailSender.SendEmailAsync(dto.To, dto.Subject, dto.Body);
            return Ok(new { message = "Email sent successfully." });
        }
    }
}
