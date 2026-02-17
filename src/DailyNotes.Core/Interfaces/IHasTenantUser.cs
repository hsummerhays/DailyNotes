namespace DailyNotes.Core.Interfaces
{
    /// <summary>
    /// Marker + contract for entities that are scoped to a tenant and user.
    /// Implement on entities that expose both <see cref="TenantId"/> and <see cref="UserId"/>.
    /// </summary>
    public interface IHasTenantUser : IHasTenant
    {
        string UserId { get; set; }
    }
}
