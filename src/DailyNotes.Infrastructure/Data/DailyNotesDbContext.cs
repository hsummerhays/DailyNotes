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

            // Work Days column mapping (PascalCase names as per manual creation)
            builder.Entity<WorkDay>(entity =>
            {
                entity.ToTable("work_days");
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.TenantId).HasColumnName("TenantId");
                entity.Property(e => e.UserId).HasColumnName("UserId");
                entity.Property(e => e.WorkDate).HasColumnName("WorkDate");
                entity.Property(e => e.TimeIn1).HasColumnName("TimeIn1");
                entity.Property(e => e.TimeOut1).HasColumnName("TimeOut1");
                entity.Property(e => e.TimeIn2).HasColumnName("TimeIn2");
                entity.Property(e => e.TimeOut2).HasColumnName("TimeOut2");
                entity.Property(e => e.TimeIn3).HasColumnName("TimeIn3");
                entity.Property(e => e.TimeOut3).HasColumnName("TimeOut3");
                entity.Property(e => e.BreakMinutes).HasColumnName("BreakMinutes");
                entity.Property(e => e.Comments).HasColumnName("Comments");
                entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt");
                entity.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt");
            });

            // Work Notes column mapping and relationship
            builder.Entity<WorkNote>(entity =>
            {
                entity.ToTable("work_notes");
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.TenantId).HasColumnName("TenantId");
                entity.Property(e => e.UserId).HasColumnName("UserId");
                entity.Property(e => e.Visibility).HasColumnName("Visibility");
                entity.Property(e => e.WorkTaskId).HasColumnName("WorkTaskId");
                entity.Property(e => e.NoteDate).HasColumnName("NoteDate");
                entity.Property(e => e.Content).HasColumnName("Content").HasColumnType("jsonb");
                entity.Property(e => e.TimeMinutes).HasColumnName("TimeMinutes");
                entity.Property(e => e.ExternalSource).HasColumnName("ExternalSource");
                entity.Property(e => e.ExternalId).HasColumnName("ExternalId");
                entity.Property(e => e.IsPinned).HasColumnName("IsPinned");
                entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt");
                entity.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt");

                // Relationship via NoteDate -> WorkDate
                entity.HasOne(n => n.WorkDay)
                      .WithMany(d => d.Notes)
                      .HasForeignKey(n => n.NoteDate)
                      .HasPrincipalKey(d => d.WorkDate);
            });

            // Work Tasks column mapping
            builder.Entity<WorkTask>(entity =>
            {
                entity.ToTable("work_tasks");
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.TenantId).HasColumnName("TenantId");
                entity.Property(e => e.UserId).HasColumnName("UserId");
                entity.Property(e => e.Visibility).HasColumnName("Visibility");
                entity.Property(e => e.Name).HasColumnName("Name");
                entity.Property(e => e.Status).HasColumnName("Status");
                entity.Property(e => e.Comments).HasColumnName("Comments");
                entity.Property(e => e.StartDate).HasColumnName("StartDate");
                entity.Property(e => e.DueDate).HasColumnName("DueDate");
                entity.Property(e => e.ProjectId).HasColumnName("ProjectId");
                entity.Property(e => e.ParentTaskId).HasColumnName("ParentTaskId");
                entity.Property(e => e.ExternalSource).HasColumnName("ExternalSource");
                entity.Property(e => e.ExternalId).HasColumnName("ExternalId");
                entity.Property(e => e.IsPinned).HasColumnName("IsPinned");
                entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt");
                entity.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt");
            });

            // Projects column mapping
            builder.Entity<Project>(entity =>
            {
                entity.ToTable("projects");
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.TenantId).HasColumnName("TenantId");
                entity.Property(e => e.UserId).HasColumnName("UserId");
                entity.Property(e => e.Visibility).HasColumnName("Visibility");
                entity.Property(e => e.Name).HasColumnName("Name");
                entity.Property(e => e.Category).HasColumnName("Category");
                entity.Property(e => e.CreatedDate).HasColumnName("CreatedDate");
                entity.Property(e => e.CompletedDate).HasColumnName("CompletedDate");
                entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt");
                entity.Property(e => e.UpdatedAt).HasColumnName("UpdatedAt");
            });

            // Pay Periods column mapping
            builder.Entity<PayPeriod>(entity =>
            {
                entity.ToTable("pay_periods");
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.TenantId).HasColumnName("TenantId");
                entity.Property(e => e.UserId).HasColumnName("UserId");
                entity.Property(e => e.PeriodStartDate).HasColumnName("PeriodStartDate");
                entity.Property(e => e.PeriodEndDate).HasColumnName("PeriodEndDate");
                entity.Property(e => e.Holidays).HasColumnName("Holidays");
                entity.Property(e => e.PtoReported).HasColumnName("PtoReported");
                entity.Property(e => e.PtoDaysOfMonth).HasColumnName("PtoDaysOfMonth");
                entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt");
            });
        }
    }
}
