using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DailyNotes.Core.Entities;
using DailyNotes.Core.Interfaces;
using DailyNotes.Application.Data;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DailyNotes.Infrastructure.Data
{
    public class DailyNotesDbContext : IdentityDbContext, IDailyNotesDataContext
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public DailyNotesDbContext(DbContextOptions<DailyNotesDbContext> options, IHttpContextAccessor? httpContextAccessor = null)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private int? CurrentTenantId
        {
            get
            {
                var claim = _httpContextAccessor?.HttpContext?.User?.FindFirstValue("tenant_id");
                return int.TryParse(claim, out var id) ? id : null;
            }
        }

        private void ApplyTenantFilter<T>(ModelBuilder builder) where T : class, IHasTenant
            => builder.Entity<T>().HasQueryFilter(e => CurrentTenantId == null || e.TenantId == CurrentTenantId);

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
        public DbSet<MemoryItem> MemoryItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasPostgresExtension("vector");

            // Global tenant isolation filters — bypassed automatically when no HTTP context
            // (migrations, background jobs, tests that call IgnoreQueryFilters())
            ApplyTenantFilter<WorkDay>(builder);
            ApplyTenantFilter<WorkNote>(builder);
            ApplyTenantFilter<WorkTask>(builder);
            ApplyTenantFilter<Topic>(builder);
            ApplyTenantFilter<TopicNote>(builder);
            ApplyTenantFilter<Course>(builder);
            ApplyTenantFilter<Assignment>(builder);
            ApplyTenantFilter<Project>(builder);
            ApplyTenantFilter<PayPeriod>(builder);
            ApplyTenantFilter<Quiz>(builder);
            ApplyTenantFilter<Tag>(builder);
            ApplyTenantFilter<MemoryItem>(builder);
            ApplyTenantFilter<QuizAttempt>(builder);

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
            builder.Entity<TopicNote>().HasIndex(tn => new { tn.TenantId, tn.UserId });
            builder.Entity<Course>().HasIndex(c => c.TenantId);
            builder.Entity<Assignment>().HasIndex(a => a.CourseId);
            builder.Entity<Assignment>().HasIndex(a => new { a.TenantId, a.UserId });

            // Missing composite indexes on high-traffic tenant-scoped tables
            builder.Entity<WorkDay>().HasIndex(w => new { w.TenantId, w.UserId });
            builder.Entity<WorkNote>().HasIndex(n => new { n.TenantId, n.UserId });
            builder.Entity<WorkTask>().HasIndex(t => new { t.TenantId, t.UserId });
            builder.Entity<PayPeriod>().HasIndex(p => new { p.TenantId, p.UserId });

            // QuizAttempt tenant scoping
            builder.Entity<QuizAttempt>().HasIndex(a => new { a.TenantId, a.UserId });

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

            builder.Entity<MemoryItem>().ToTable("memory_items");
            builder.Entity<MemoryItem>().HasIndex(m => m.TenantId);
            builder.Entity<MemoryItem>().HasIndex(m => new { m.TenantId, m.UserId });
            builder.Entity<MemoryItem>().Property(m => m.MemoryStatus).HasDefaultValue("Active");

            if (Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory" || Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                builder.Entity<MemoryItem>()
                    .Property(m => m.Embedding);
            }
            else
            {
                var vectorConverter = new ValueConverter<float[], Pgvector.Vector>(
                    v => new Pgvector.Vector(v),
                    v => v.ToArray());

                var vectorComparer = new ValueComparer<float[]>(
                    (a, b) => (a ?? Array.Empty<float>()).SequenceEqual(b ?? Array.Empty<float>()),
                    v => v.Aggregate(0, (hash, x) => HashCode.Combine(hash, x)),
                    v => v.ToArray());

                builder.Entity<MemoryItem>()
                    .Property(m => m.Embedding)
                    .HasColumnType($"vector({MemoryItem.EmbeddingDimensions})")
                    .HasConversion(vectorConverter, vectorComparer);
            }

            builder.Entity<MemoryItem>()
                .HasOne(m => m.RelatedMemory)
                .WithMany()
                .HasForeignKey(m => m.RelatedMemoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Work Notes relationship (NoteDate -> WorkDate FK)
            builder.Entity<WorkNote>(entity =>
            {
                entity.Property(e => e.Content).HasColumnType("jsonb");

                // Relationship via NoteDate -> WorkDate
                entity.HasOne(n => n.WorkDay)
                      .WithMany(d => d.Notes)
                      .HasForeignKey(n => n.NoteDate)
                      .HasPrincipalKey(d => d.WorkDate);
            });

            // SQLite / InMemory fallback for JsonDocument types
            if (Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory" || Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                var jsonConverter = new ValueConverter<JsonDocument, string>(
                    v => v.RootElement.ToString(),
                    v => JsonDocument.Parse(v, default));

                builder.Entity<TenantUser>()
                    .Property(e => e.Preferences)
                    .HasConversion(jsonConverter);

                builder.Entity<WorkNote>()
                    .Property(e => e.Content)
                    .HasConversion(jsonConverter);

                builder.Entity<TopicNote>()
                    .Property(e => e.Content)
                    .HasConversion(jsonConverter);

                builder.Entity<WebhookEvent>()
                    .Property(e => e.Payload)
                    .HasConversion(jsonConverter);
            }
        }
    }
}
