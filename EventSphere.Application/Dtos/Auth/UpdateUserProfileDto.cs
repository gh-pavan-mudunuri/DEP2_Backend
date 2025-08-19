using Microsoft.AspNetCore.Http;

namespace EventSphere.Application.Dtos.Auth
{
    public class UpdateUserProfileDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public IFormFile? ProfileImage { get; set; }
    }
}
