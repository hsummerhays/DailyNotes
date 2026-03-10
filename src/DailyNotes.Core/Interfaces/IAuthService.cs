using System.Threading.Tasks;
using DailyNotes.Core.DTOs.Auth;

namespace DailyNotes.Core.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterDto model);
        Task<AuthResult> LoginAsync(LoginDto model);
        Task<AuthResult> RefreshTokenAsync(string refreshToken);
    }
}
