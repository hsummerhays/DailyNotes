using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DailyNotes.Core.Entities;
using System.Text.Json;

namespace DailyNotes.Infrastructure.Data
{
    public class DailyNotesDbContext : IdentityDbContext
    {
        public DailyNotesDbContext(DbContextOptions<DailyNotesDbContext> options)
            : base(options)
        {
        }

        public DbSet<Tenant> Tenants { get; set; } = null!;
        public DbSet<TenantUser> TenantUsers { get; set; } = null!;
        public DbSet<WorkDay> WorkDays { get; set; } = null!;
        public DbSet<WorkTask> WorkTasks { get; set; } = null!;
        public DbSet<WorkNote> WorkNotes { get; set; } = null!;
        public DbSet<Topic> Topics { get; set; } = null!;
        public DbSet<TopicNote> TopicNotes { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<ItemTag> ItemTags { get; set; } = null!;
        public DbSet<Course> Courses { get; set; } = null!;
        public DbSet<Assignment> Assignments { get; set; } = null!;
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<PayPeriod> PayPeriods { get; set; } = null!;
        public DbSet<SharedItem> SharedItems { get; set; } = null!;
        public DbSet<Attachment> Attachments { get; set; } = null!;
        public DbSet<Quiz> Quizzes { get; set; } = null!;
        public DbSet<QuizQuestion> QuizQuestions { get; set; } = null!;
        public DbSet<QuizOption> QuizOptions { get; set; } = null!;
        public DbSet<QuizAttempt> QuizAttempts { get; set; } = null!;
        public DbSet<QuizAnswer> QuizAnswers { get; set; } = null!;
        public DbSet<IntegrationConnection> IntegrationConnections { get; set; } = null!;
        public DbSet<WebhookEvent> WebhookEvents { get; set; } = null!;
        public DbSet<ApiKey> ApiKeys { get; set; } = null!;
        public DbSet<WebhookSubscription> WebhookSubscriptions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Tenant User key
            builder.Entity<TenantUser>()
                .HasKey(tu => new { tu.TenantId, tu.UserId });

            // JSONB conversions
            builder.Entity<TenantUser>()
                .Property(e => e.Preferences)
                .HasColumnType("jsonb");

            builder.Entity<WorkNote>()
                .Property(e => e.Content)
                .HasColumnType("jsonb");

            builder.Entity<TopicNote>()
                .Property(e => e.Content)
                .HasColumnType("jsonb");

            // ItemTag key
            builder.Entity<ItemTag>()
                .HasKey(it => new { it.TagId, it.ItemType, it.ItemId });

            // Indexes & Unique constraints
            builder.Entity<Tag>()
                .HasIndex(t => new { t.TenantId, t.Name })
                .IsUnique();

            // Rename Identity tables to snake_case
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUser>(entity => entity.ToTable("asp_net_users"));
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>(entity => entity.ToTable("asp_net_roles"));
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>(entity => entity.ToTable("asp_net_user_roles"));
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>(entity => entity.ToTable("asp_net_user_claims"));
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>(entity => entity.ToTable("asp_net_user_logins"));
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>>(entity => entity.ToTable("asp_net_role_claims"));
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<string>>(entity => entity.ToTable("asp_net_user_tokens"));

            // Table names implementation
            builder.Entity<Tenant>().ToTable("tenants");
            builder.Entity<TenantUser>().ToTable("tenant_users");
            builder.Entity<WorkDay>().ToTable("work_days");
            builder.Entity<WorkTask>().ToTable("work_tasks");
            builder.Entity<WorkNote>().ToTable("work_notes");
            builder.Entity<Topic>().ToTable("topics");
            builder.Entity<TopicNote>().ToTable("topic_notes");
            builder.Entity<Tag>().ToTable("tags");
            builder.Entity<ItemTag>().ToTable("item_tags");
            builder.Entity<Course>().ToTable("courses");
            builder.Entity<Assignment>().ToTable("assignments");
            builder.Entity<Project>().ToTable("projects");
            builder.Entity<PayPeriod>().ToTable("pay_periods");
            builder.Entity<SharedItem>().ToTable("shared_items");
            builder.Entity<Attachment>().ToTable("attachments");
            builder.Entity<Quiz>().ToTable("quizzes");
            builder.Entity<QuizQuestion>().ToTable("quiz_questions");
            builder.Entity<QuizOption>().ToTable("quiz_options");
            builder.Entity<QuizAttempt>().ToTable("quiz_attempts");
            builder.Entity<QuizAnswer>().ToTable("quiz_answers");

            // Keys & Relationships
            builder.Entity<QuizAnswer>()
                .HasKey(qa => new { qa.AttemptId, qa.QuestionId });

            builder.Entity<QuizQuestion>()
                .HasOne<Quiz>()
                .WithMany()
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<QuizOption>()
                .HasOne<QuizQuestion>()
                .WithMany()
                .HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<QuizAnswer>()
                .HasOne<QuizAttempt>()
                .WithMany()
                .HasForeignKey(a => a.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.Entity<Project>().HasIndex(p => p.TenantId);
            builder.Entity<SharedItem>().HasIndex(si => new { si.ItemType, si.ItemId });
            builder.Entity<SharedItem>().HasIndex(si => si.SharedWithUserId);
            builder.Entity<Attachment>().HasIndex(a => new { a.ItemType, a.ItemId });
            builder.Entity<Topic>().HasIndex(t => t.TenantId);
            builder.Entity<Topic>().HasIndex(t => t.ParentTopicId);
            builder.Entity<TopicNote>().HasIndex(tn => tn.TopicId);
            builder.Entity<Course>().HasIndex(c => c.TenantId);
            builder.Entity<Assignment>().HasIndex(a => a.CourseId);

            // Integration entities
            builder.Entity<IntegrationConnection>().ToTable("integration_connections");
            builder.Entity<IntegrationConnection>()
                .HasIndex(ic => new { ic.TenantId, ic.Provider })
                .IsUnique();

            builder.Entity<WebhookEvent>().ToTable("webhook_events");
            builder.Entity<WebhookEvent>()
                .Property(e => e.Payload)
                .HasColumnType("jsonb");

            builder.Entity<ApiKey>().ToTable("api_keys");

            builder.Entity<WebhookSubscription>().ToTable("webhook_subscriptions");
        }
    }
}
