using DailyNotes.Core.DTOs.Auth;
using DailyNotes.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DailyNotes.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private const string CookieName = "refreshToken";
        private readonly IAuthService _authService;
        private readonly IWebHostEnvironment _env;

        public AuthController(IAuthService authService, IWebHostEnvironment env)
        {
            _authService = authService;
            _env = env;
        }

        /// <summary>Registers a new user account.</summary>
        [HttpPost("register")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            var result = await _authService.RegisterAsync(model);
            SetRefreshTokenCookie(result.RefreshToken);
            return Ok(result.Response);
        }

        /// <summary>Authenticates a user and returns a JWT access token.</summary>
        [HttpPost("login")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var result = await _authService.LoginAsync(model);
            SetRefreshTokenCookie(result.RefreshToken);
            return Ok(result.Response);
        }

        /// <summary>Refreshes an expired JWT using the httpOnly refresh token cookie.</summary>
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies[CookieName];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new { message = "No refresh token provided." });

            var result = await _authService.RefreshTokenAsync(refreshToken);
            SetRefreshTokenCookie(result.RefreshToken);
            return Ok(result.Response);
        }

        /// <summary>Logs out the current user by clearing the refresh token cookie.</summary>
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete(CookieName, MakeCookieOptions());
            return NoContent();
        }

        private void SetRefreshTokenCookie(string token)
        {
            var opts = MakeCookieOptions();
            opts.Expires = DateTimeOffset.UtcNow.AddDays(30);
            Response.Cookies.Append(CookieName, token, opts);
        }

        private CookieOptions MakeCookieOptions() => new()
        {
            HttpOnly = true,
            Secure = !_env.IsDevelopment(),  // HTTPS-only in production
            SameSite = SameSiteMode.Strict,
            Path = "/"
        };
    }
}
