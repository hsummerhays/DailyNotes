namespace DailyNotes.Application
{
    public interface ITenantContext
    {
        string UserId { get; }
        int TenantId { get; }
    }
}
