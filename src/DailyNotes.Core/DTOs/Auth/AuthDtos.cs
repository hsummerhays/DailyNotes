using System.ComponentModel.DataAnnotations;

namespace DailyNotes.Core.DTOs.Auth
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [MaxLength(128)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? DisplayName { get; set; }
    }

    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public string TenantId { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    // Internal wrapper: controller reads RefreshToken to set cookie, returns Response as body
    public class AuthResult
    {
        public AuthResponseDto Response { get; init; } = new();
        public string RefreshToken { get; init; } = string.Empty;
    }
}
