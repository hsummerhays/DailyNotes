using DailyNotes.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DailyNotes.Application.Data
{
    public interface IDailyNotesDataContext
    {
        DbSet<Tenant> Tenants { get; }
        DbSet<TenantUser> TenantUsers { get; }
        DbSet<WorkDay> WorkDays { get; }
        DbSet<WorkNote> WorkNotes { get; }
        DbSet<WorkTask> WorkTasks { get; }
        DbSet<Topic> Topics { get; }
        DbSet<TopicNote> TopicNotes { get; }
        DbSet<Tag> Tags { get; }
        DbSet<ItemTag> ItemTags { get; }
        DbSet<Course> Courses { get; }
        DbSet<Assignment> Assignments { get; }
        DbSet<Project> Projects { get; }
        DbSet<PayPeriod> PayPeriods { get; }
        DbSet<SharedItem> SharedItems { get; }
        DbSet<Attachment> Attachments { get; }
        DbSet<Quiz> Quizzes { get; }
        DbSet<QuizQuestion> QuizQuestions { get; }
        DbSet<QuizOption> QuizOptions { get; }
        DbSet<QuizAttempt> QuizAttempts { get; }
        DbSet<QuizAnswer> QuizAnswers { get; }
        DbSet<IntegrationConnection> IntegrationConnections { get; }
        DbSet<WebhookEvent> WebhookEvents { get; }
        DbSet<ApiKey> ApiKeys { get; }
        DbSet<WebhookSubscription> WebhookSubscriptions { get; }
        DbSet<MemoryItem> MemoryItems { get; }

        DatabaseFacade Database { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
