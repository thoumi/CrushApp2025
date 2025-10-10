using MediaService.Data;
using MediaService.Events;
using MediaService.Models;
using MediaService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

// Configuration Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.WithProperty("Service", "MediaService")
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Seq(Environment.GetEnvironmentVariable("SEQ_URL") ?? "http://seq:5341")
    .CreateLogger();

try
{
    Log.Information("🖼️ Starting MediaService...");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog();

    // Add services
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Database
    builder.Services.AddDbContext<MediaDbContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    });

    // Cloudinary
    builder.Services.Configure<CloudinarySettings>(
        builder.Configuration.GetSection("CloudinarySettings"));
    builder.Services.AddScoped<IPhotoService, PhotoService>();

    // RabbitMQ Event Publisher
    builder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    // JWT Authentication
    var tokenKey = builder.Configuration["TokenKey"] ?? 
        throw new Exception("TokenKey not found in configuration");
    
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });

    builder.Services.AddAuthorization();

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<MediaDbContext>();

    var app = builder.Build();

    // Configure pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/ready");
    
    app.MapGet("/info", () => new
    {
        service = "MediaService",
        version = "1.0.0",
        status = "running",
        timestamp = DateTime.UtcNow
    });

    // Apply migrations
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
            await db.Database.MigrateAsync();
            Log.Information("✅ Database migrations applied");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "❌ Error applying migrations");
        }
    }

    Log.Information("✅ MediaService started successfully on {Urls}", 
        builder.Configuration["ASPNETCORE_URLS"] ?? "unknown");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ MediaService terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

