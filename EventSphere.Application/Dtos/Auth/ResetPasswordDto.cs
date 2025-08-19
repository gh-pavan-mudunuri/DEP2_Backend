using System.ComponentModel.DataAnnotations;
namespace backend.Dtos
{
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "Token is required.")]
        public string? Token { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        public string? NewPassword { get; set; }
    }
}
