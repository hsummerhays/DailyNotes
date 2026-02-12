namespace DailyNotes.Core.Entities
{
    public class ItemTag
    {
        public int TagId { get; set; }
        public string ItemType { get; set; } = string.Empty; // 'note', 'task', 'topic', 'course'
        public int ItemId { get; set; }
        public int TenantId { get; set; }
    }
}
