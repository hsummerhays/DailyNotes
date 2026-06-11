using System.Text;
using DailyNotes.Application.Data;
using DailyNotes.Application.Services;
using DailyNotes.Core.Interfaces;
using DailyNotes.Infrastructure.Data;
using DailyNotes.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace DailyNotes.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IWorkDayService, WorkDayService>();
            services.AddScoped<IWorkNoteService, WorkNoteService>();
            services.AddScoped<IWorkTaskService, WorkTaskService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<ICourseService, CourseService>();
            services.AddScoped<ITopicService, TopicService>();
            services.AddScoped<ITopicNoteService, TopicNoteService>();
            services.AddScoped<ITagService, TagService>();
            services.AddScoped<IAssignmentService, AssignmentService>();
            services.AddScoped<IAttachmentService, AttachmentService>();
            services.AddScoped<IPayPeriodService, PayPeriodService>();
            services.AddScoped<IQuizService, QuizService>();
            services.AddScoped<IQuizAttemptService, QuizAttemptService>();
            services.AddScoped<ISearchService, SearchService>();

            return services;
        }

        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            if (!environment.IsEnvironment("Testing"))
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

                services.AddDbContext<DailyNotesDbContext>(options =>
                    options.UseNpgsql(connectionString));
            }

            services.AddIdentityCore<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<DailyNotesDbContext>();

            services.AddScoped<IDailyNotesDataContext>(sp => sp.GetRequiredService<DailyNotesDbContext>());
            services.AddScoped<IAuthService, AuthService>();

            // Stub provider implementations (replace with real ones when ready)
            services.AddScoped<IEmailProvider, NullEmailProvider>();
            services.AddScoped<IFileStorageProvider, NullFileStorageProvider>();
            services.AddScoped<IAiVisionProvider, NullAiVisionProvider>();
            services.AddScoped<ISpeechProvider, NullSpeechProvider>();

            services.AddSingleton(TimeProvider.System);

            return services;
        }

        public static IServiceCollection AddAuthConfiguration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidAudience = configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
                    };
                });

            services.AddAuthorization();

            return services;
        }

        public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "DailyNotes API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });
                c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecuritySchemeReference("Bearer", doc, null),
                        new List<string>()
                    }
                });

                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            return services;
        }
    }
}
