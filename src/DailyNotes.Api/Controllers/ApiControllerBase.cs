using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using DailyNotes.Core.Interfaces;

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

        /// <summary>
        /// Scope an IQueryable to the current tenant and user.
        /// Usage: TenantScoped(_context.YourDbSet)
        /// </summary>
        protected IQueryable<T> TenantScoped<T>(IQueryable<T> query) where T : class, IHasTenantUser
        {
            var tenantId = CurrentTenantId;
            var userId = CurrentUserId;
            return query.Where(e => e.TenantId == tenantId && e.UserId == userId);
        }

        /// <summary>
        /// Scope an IQueryable to the current tenant only.
        /// Usage: TenantOnlyScoped(_context.Tags)
        /// </summary>
        protected IQueryable<T> TenantOnlyScoped<T>(IQueryable<T> query) where T : class, IHasTenant
        {
            var tenantId = CurrentTenantId;
            return query.Where(e => e.TenantId == tenantId);
        }
    }
}
