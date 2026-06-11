using DailyNotes.Application.Data;
using DailyNotes.Core.Interfaces;

namespace DailyNotes.Application.Services
{
    public abstract class ApplicationServiceBase
    {
        protected readonly IDailyNotesDataContext _db;
        protected readonly ITenantContext _tc;
        protected readonly TimeProvider _clock;

        protected ApplicationServiceBase(IDailyNotesDataContext db, ITenantContext tc, TimeProvider clock)
        {
            _db = db;
            _tc = tc;
            _clock = clock;
        }

        protected IQueryable<T> TenantScoped<T>(IQueryable<T> query) where T : class, IHasTenantUser
            => query.Where(e => e.TenantId == _tc.TenantId && e.UserId == _tc.UserId);

        protected IQueryable<T> TenantOnlyScoped<T>(IQueryable<T> query) where T : class, IHasTenant
            => query.Where(e => e.TenantId == _tc.TenantId);
    }
}
