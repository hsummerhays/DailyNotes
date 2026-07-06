using System.Net;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using DailyNotes.Api.Extensions;
using DailyNotes.Api.Infrastructure;
using DailyNotes.Application;
using DailyNotes.Core.Exceptions;
using DailyNotes.Infrastructure.Data;
using DailyNotes.Infrastructure.Helpers;
using Microsoft.AspNetCore.RateLimiting;

EnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddSwaggerConfiguration();

builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment);

builder.Services.AddAuthConfiguration(builder.Configuration);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowDevClient", policy =>
        policy.WithOrigins("http://localhost:5173", "http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// HTTP context accessor (required by HttpTenantContext)
builder.Services.AddHttpContextAccessor();

// Tenant context — resolves current user/tenant from JWT claims per request
builder.Services.AddScoped<ITenantContext, HttpTenantContext>();

builder.Services.AddApplicationServices();

var app = builder.Build();

// Configure the HTTP request pipeline.

// Global error handling
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.ContentType = "application/json";

        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (error != null)
        {
            if (error.Error is DomainException domainEx)
            {
                context.Response.StatusCode = domainEx.StatusCode;
                await context.Response.WriteAsJsonAsync(new { message = domainEx.Message });
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                var env = context.RequestServices.GetRequiredService<IHostEnvironment>();
                var response = env.IsDevelopment()
                    ? new { message = error.Error.Message, detail = error.Error.StackTrace }
                    : new { message = "An unexpected error occurred.", detail = (string?)null };
                await context.Response.WriteAsJsonAsync(response);
            }
        }
    });
});

app.UseCors("AllowDevClient");
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
