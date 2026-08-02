using API.Api.Filters;
using API.Api.Middleware;
using API.Api.SignalR;
using API.Application.Events;
using API.Application.Helpers;
using API.Application.Interfaces;
using API.Application.Services;
using API.Application.Validators;
using API.Domain.Entities;
using API.Infrastructure.Data;
using API.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

// Configuration Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.WithProperty("Service", "CoreAPI")
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Seq(Environment.GetEnvironmentVariable("SEQ_URL") ?? "http://localhost:5341")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Intégrer Serilog
builder.Host.UseSerilog();

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddCors();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPhotoService, PhotoService>();
// UnitOfWork est enregistré une seule fois (une seule instance scoped = un seul DbContext =
// une seule transaction par requête). Chaque brique de compétence résout la même instance,
// pour que chaque consommateur ne dépende que de ce qu'il utilise réellement (ISP).
builder.Services.AddScoped<UnitOfWork>();
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<UnitOfWork>());
builder.Services.AddScoped<IMemberUnitOfWork>(sp => sp.GetRequiredService<UnitOfWork>());
builder.Services.AddScoped<IMessagingUnitOfWork>(sp => sp.GetRequiredService<UnitOfWork>());
builder.Services.AddScoped<ILikesUnitOfWork>(sp => sp.GetRequiredService<UnitOfWork>());
builder.Services.AddScoped<IMatchingUnitOfWork>(sp => sp.GetRequiredService<UnitOfWork>());
builder.Services.AddScoped<IModerationUnitOfWork>(sp => sp.GetRequiredService<UnitOfWork>());
builder.Services.AddScoped<IMatchingService, MatchingService>();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();
builder.Services.AddScoped<LogUserActivity>();
builder.Services.Configure<CloudinarySettings>(builder.Configuration
    .GetSection("CloudinarySettings"));
builder.Services.AddSignalR();
builder.Services.AddSingleton<PresenceTracker>();
builder.Services.AddMemoryCache();

// RabbitMQ Event Consumer
builder.Services.AddSingleton<IEventConsumer, RabbitMqEventConsumer>();
builder.Services.AddHostedService<PhotoEventConsumerService>();

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database");

builder.Services.AddIdentityCore<AppUser>(opt =>
{
    opt.Password.RequireNonAlphanumeric = false;
    opt.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var tokenKey = builder.Configuration["TokenKey"]
            ?? throw new Exception("Token key not found - Program.cs");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
            ValidateIssuer = false,
            ValidateAudience = false
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception?.Message}");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine($"Challenge: {context.ErrorDescription}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"))
    .AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("Admin", "Moderator"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Halo API",
        Version = "v1",
        Description = "API cœur de Halo (membres, likes/matchs, messagerie, prompts)."
    });

    var jwtScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Entrez le token JWT obtenu via /api/account/login",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    options.AddSecurityDefinition("Bearer", jwtScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwtScheme, [] } });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Halo API v1");
});

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors(x => x
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
    .WithOrigins("http://localhost:4200", "https://localhost:4200", "http://localhost:8080", "https://localhost:8080"));

app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapHub<PresenceHub>("hubs/presence");
app.MapHub<MessageHub>("hubs/messages");

// Health Checks endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

// Info endpoint
app.MapGet("/info", () => new
{
    service = "CoreAPI",
    version = "1.0.0",
    status = "running",
    timestamp = DateTime.UtcNow
});

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    Console.WriteLine("🔄 Starting database initialization...");
    
    var context = services.GetRequiredService<AppDbContext>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    
    Console.WriteLine("📊 Applying database migrations...");
    await context.Database.MigrateAsync();
    Console.WriteLine("✅ Database migrations applied");
    
    Console.WriteLine("🧹 Clearing old connections...");
    await context.Connections.ExecuteDeleteAsync();
    
    Console.WriteLine("🌱 Running database seed...");
    await Seed.SeedUsers(userManager);
    
    Console.WriteLine("✅ Database initialization completed successfully");
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "❌ An error occurred during database initialization");
    Console.WriteLine($"❌ Database initialization failed: {ex.Message}");
}

try
{
    Log.Information("✅ Core API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Core API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
