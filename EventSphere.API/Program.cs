// Register SmtpEmailSender for DI
using backend.Helpers;
using backend.Data;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using backend.Config;
using backend.Middleware;
using EventSphere.Application.Services;
using EventSphere.Application.Repositories;

using backend.Interfaces;
using EventSphere.Infrastructure.Repositories;
// ...existing code...
var builder = WebApplication.CreateBuilder(args);
// Register NotificationService for INotificationService
builder.Services.AddScoped<EventSphere.Application.Interfaces.INotificationService, EventSphere.Infrastructure.Services.NotificationService>();
builder.Configuration.AddEnvironmentVariables();
builder.Services.AddScoped<SmtpEmailSender>();
// Add CORS policy for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins(
                "http://localhost:3000", // Next.js default
                "http://localhost:5173",
                "https://dep-2-frontend-d1478v9w0-pavans-projects-4424e67e.vercel.app",
               "https://dep-2-frontend-jbqpyylbd-pavans-projects-4424e67e.vercel.app",
               "https://dep-2-frontend.vercel.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    );
});
builder.Services.AddScoped<EventSphere.Application.Interfaces.IPaymentService, EventSphere.Application.Services.PaymentService>();
builder.Services.AddScoped<IAdminService, backend.Services.AdminService>();
// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
// Register DashboardService for DI
builder.Services.AddScoped<IBookmarkRepository, BookmarkRepository>();
builder.Services.AddScoped<EventSphere.Application.Interfaces.IBookmarkService, backend.Services.BookmarkService>();

builder.Services.AddScoped<EventSphere.Application.Interfaces.IDashboardService, EventSphere.Application.Services.DashboardService>();

// If your implementation is HomeService:
builder.Services.AddScoped<backend.Interfaces.IHomeService, backend.Services.HomeService>();

// Add JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        var jwtKey = builder.Configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 16)
            throw new InvalidOperationException("JWT key is missing or too short. Set a strong key (min 16 chars) in configuration.");
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey!))
        };
    });

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("EventSphereDb"));
}
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<backend.Interfaces.IEventService, backend.Services.EventService>();


// Register repositories for DI
builder.Services.AddScoped<EventSphere.Application.Repositories.IAuthRepository, EventSphere.Infrastructure.Repositories.AuthRepository>();
builder.Services.AddScoped<EventSphere.Application.Repositories.IEventRepository, EventSphere.Infrastructure.Repositories.EventRepository>();
builder.Services.AddScoped<EventSphere.Application.Interfaces.IDashboardService, EventSphere.Application.Services.DashboardService>();
builder.Services.AddScoped<EventSphere.Application.Repositories.IPaymentRepository, EventSphere.Infrastructure.Repositories.PaymentRepository>();
builder.Services.AddScoped<EventSphere.Application.Interfaces.IRegistrationService, EventSphere.Application.Services.RegistrationService>();
builder.Services.AddScoped<EventSphere.Application.Repositories.IRegistrationRepository, EventSphere.Infrastructure.Repositories.RegistrationRepository>();
builder.Services.AddScoped<EventSphere.Infrastructure.Repositories.StatsRepository>();
builder.Services.AddScoped<EventSphere.Application.Interfaces.IStatsService, EventSphere.Application.Services.StatsService>();
builder.Services.AddScoped<EventSphere.Infrastructure.Repositories.WebsiteReviewRepository>();
builder.Services.AddScoped<EventSphere.Application.Interfaces.IWebsiteReviewService, EventSphere.Application.Services.WebsiteReviewService>();

builder.Services.AddHostedService<HourlyEventCompletionService>();
builder.Services.AddHostedService<ScheduledReminderService>();
builder.Services.AddHostedService<HourlyEventReminderService>();


// Set Stripe API key globally after builder is created and before app is built
var stripeSecretKey = builder.Configuration["STRIPE_SECRET_KEY"];
Stripe.StripeConfiguration.ApiKey = stripeSecretKey;
Console.WriteLine($"[DEBUG] STRIPE_SECRET_KEY: {stripeSecretKey}");


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
    c.RoutePrefix = "swagger"; // Optional, default is "swagger"
});


app.UseCors("AllowFrontend");

// Serve files from wwwroot
app.UseStaticFiles();

// Serve /uploads/* from wwwroot/uploads

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "uploads")),
    RequestPath = "/uploads",
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "http://localhost:3000");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
    }
});


// Register middlewares in recommended order
app.UseErrorHandlingMiddleware();
app.UseLoggingMiddleware();
app.UsePerformanceLoggingMiddleware();
app.UseRateLimitingMiddleware();
app.UseRequestValidationMiddleware();
app.UseJwtMiddleware();
app.UseResponseFormattingMiddleware();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
