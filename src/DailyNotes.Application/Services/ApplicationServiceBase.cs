using DailyNotes.Core.Interfaces;
using DailyNotes.Infrastructure.Data;

namespace DailyNotes.Application.Services
{
    public abstract class ApplicationServiceBase
    {
        protected readonly DailyNotesDbContext _db;
        protected readonly ITenantContext _tc;

        protected ApplicationServiceBase(DailyNotesDbContext db, ITenantContext tc)
        {
            _db = db;
            _tc = tc;
        }

        protected IQueryable<T> TenantScoped<T>(IQueryable<T> query) where T : class, IHasTenantUser
            => query.Where(e => e.TenantId == _tc.TenantId && e.UserId == _tc.UserId);

        protected IQueryable<T> TenantOnlyScoped<T>(IQueryable<T> query) where T : class, IHasTenant
            => query.Where(e => e.TenantId == _tc.TenantId);
    }
}
