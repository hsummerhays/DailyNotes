using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authentication;
using DailyNotes.Infrastructure.Data;
using DailyNotes.Core.Entities;

namespace DailyNotes.Api.Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbName = "DailyNotes_Test_" + Guid.NewGuid();

        public CustomWebApplicationFactory()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Add InMemory database (Npgsql was skipped in Program.cs due to "Testing" env)
                services.AddDbContext<DailyNotesDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_dbName);
                    options.ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
                });

                // Replace authentication
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

                // Build SP and seed
                // Note: BuildServiceProvider is called to seed data. 
                // Since Npgsql was never added, there's no conflict.
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<DailyNotesDbContext>();

                // Ensure a clean, seeded DB at startup
                ResetDatabase(db);
            });
        }

        // Helper to seed the provided DbContext instance
        private static void ResetDatabase(DailyNotesDbContext db)
        {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            var t1 = new Tenant { Name = "Tenant One", Id = 1 };
            var t2 = new Tenant { Name = "Tenant Two", Id = 2 };
            db.Tenants.AddRange(t1, t2);
            db.SaveChanges();

            var wd1 = new WorkDay
            {
                TenantId = t1.Id,
                UserId = "user-a",
                WorkDate = DateOnly.FromDateTime(DateTime.UtcNow),
                BreakMinutes = 30,
                Comments = "Tenant1 - user-a"
            };

            var wd2 = new WorkDay
            {
                TenantId = t2.Id,
                UserId = "user-b",
                WorkDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                BreakMinutes = 0,
                Comments = "Tenant2 - user-b"
            };

            db.WorkDays.AddRange(wd1, wd2);
            db.SaveChanges();
        }

        // Public method tests can call to reset DB back to seeded state
        public void ResetDatabase()
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DailyNotesDbContext>();
            ResetDatabase(db);
        }
    }
}
