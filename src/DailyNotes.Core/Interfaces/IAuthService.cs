using System.Threading.Tasks;
using DailyNotes.Core.DTOs.Auth;

namespace DailyNotes.Core.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto model);
        Task<AuthResponseDto> LoginAsync(LoginDto model);
        // Task<AuthResponseDto> RefreshTokenAsync(string token, string refreshToken); // Build later
    }
}
