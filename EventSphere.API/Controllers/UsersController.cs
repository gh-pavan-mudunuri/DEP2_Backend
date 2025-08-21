// ...existing code...

    using Microsoft.AspNetCore.Mvc;
    using backend.Services;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using EventSphere.Application.Dtos.Auth;

namespace EventSphere.API.Controllers
{
    public class StripeOnboardingRequestDto
    {
        public required string Email { get; set; }
    }
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
            private readonly IUserService _userService;
            private readonly IFileService _fileService;
            private readonly EventSphere.Application.Repositories.IPaymentRepository _paymentRepository;

            public UsersController(IUserService userService, IFileService fileService, EventSphere.Application.Repositories.IPaymentRepository paymentRepository)
            {
                _userService = userService;
                _fileService = fileService;
                _paymentRepository = paymentRepository;
            }

            // GET: api/users/{id}
            [HttpGet("{id}")]
            public async Task<IActionResult> GetUserById(int id)
            {
                var dto = await _userService.GetUserDetailsByIdAsync(id);
                if (dto == null)
                    return NotFound();
                return Ok(dto);
            }

            // PATCH: api/users/{id}
            [HttpPatch("{id}")]
            public async Task<IActionResult> UpdateUser(int id, [FromForm] UpdateUserProfileDto dto)
            {
                string? imagePath = null;
                if (dto.ProfileImage != null && dto.ProfileImage.Length > 0)
                {
                    imagePath = await _fileService.SaveUserProfileImageAsync(id, dto.ProfileImage);
                }
                var result = await _userService.UpdateUserDetailsWithImageAsync(id, dto.Name, dto.Email, dto.PhoneNumber, imagePath);
                if (!result)
                    return NotFound();
                var updatedUserDetails = await _userService.GetUserDetailsByIdAsync(id);
                return Ok(updatedUserDetails);
            }

            // DELETE: api/users/{id}
            [HttpDelete("{id}")]
            public async Task<IActionResult> DeleteUser(int id)
            {
                var deleted = await _userService.DeleteUserAsync(id);
                if (!deleted)
                    return NotFound();
                return NoContent();
            }

            // POST: api/users/{id}/stripe-express-account
            [HttpPost("{id}/stripe-express-account")]
            public async Task<IActionResult> CreateStripeExpressAccount(int id, [FromBody] StripeOnboardingRequestDto dto)
            {
                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                    return NotFound(new { success = false, message = "User not found." });

                var returnUrl = "https://dep-2-frontend.vercel.app/stripe-onboarding-success";
                var refreshUrl = "https://dep-2-frontend.vercel.app/stripe-onboarding-start";

                var (accountId, onboardingUrl) = await _paymentRepository.CreateStripeExpressAccountAsync(dto.Email, returnUrl, refreshUrl);

                user.StripeAccountId = accountId;
                await _userService.UpdateUserDetailsWithImageAsync(id, user.Name, user.Email, user.Phone, user.ProfileImage); // You may want a dedicated update method

                return Ok(new { onboardingUrl });
            }

            // GET: api/users/{id}/stripe-onboarding-status
            [HttpGet("{id}/stripe-onboarding-status")]
            public async Task<IActionResult> GetStripeOnboardingStatus(int id)
            {
                var user = await _userService.GetByIdAsync(id);
                if (user == null || string.IsNullOrWhiteSpace(user.StripeAccountId))
                    return NotFound(new { success = false, message = "User or Stripe account not found." });

                var isComplete = await _paymentRepository.IsStripeAccountOnboardingCompleteAsync(user.StripeAccountId);
                return Ok(new { success = true, payoutsEnabled = isComplete });
            }
        }
    }
