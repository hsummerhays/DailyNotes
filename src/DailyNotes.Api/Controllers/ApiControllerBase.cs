using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DailyNotes.Api.Controllers
{
    public abstract class ApiControllerBase : ControllerBase
    {
        protected string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("User ID claim not found.");

        protected int CurrentTenantId
        {
            get
            {
                var claim = User.FindFirstValue("tenant_id");
                if (int.TryParse(claim, out var id)) return id;
                throw new UnauthorizedAccessException("Tenant ID claim not found or invalid.");
            }
        }
    }
}
