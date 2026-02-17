using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DailyNotes.Api.Tests
{
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Expecting headers: X-User-Id and X-Tenant-Id set by the test client
            var userId = Request.Headers["X-User-Id"].ToString();
            var tenantId = Request.Headers["X-Tenant-Id"].ToString();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tenantId))
                return Task.FromResult(AuthenticateResult.Fail("Missing test auth headers."));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim("tenant_id", tenantId),
                new Claim(ClaimTypes.Email, $"{userId}@example.test")
            };

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
