using System.Security.Claims;
using DailyNotes.Application;

namespace DailyNotes.Api.Infrastructure
{
    public class HttpTenantContext : ITenantContext
    {
        public string UserId { get; }
        public int TenantId { get; }

        public HttpTenantContext(IHttpContextAccessor accessor)
        {
            var user = accessor.HttpContext?.User
                ?? throw new UnauthorizedAccessException("No HTTP context available.");

            UserId = user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("sub")
                ?? throw new UnauthorizedAccessException("User ID claim not found.");

            var claim = user.FindFirstValue("tenant_id");
            if (!int.TryParse(claim, out var id))
                throw new UnauthorizedAccessException("Tenant ID claim not found or invalid.");

            TenantId = id;
        }
    }
}
