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

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<TenantUser> TenantUsers { get; set; }
        public DbSet<WorkDay> WorkDays { get; set; }
        public DbSet<WorkTask> WorkTasks { get; set; }
        public DbSet<WorkNote> WorkNotes { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<TopicNote> TopicNotes { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ItemTag> ItemTags { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Assignment> Assignments { get; set; }

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
        }
    }
}
