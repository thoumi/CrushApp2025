using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// Configuration de Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.WithProperty("Service", "ApiGateway")
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Seq(Environment.GetEnvironmentVariable("SEQ_URL") ?? "http://seq:5341")
    .CreateLogger();

try
{
    Log.Information("🌉 Starting API Gateway...");

    var builder = WebApplication.CreateBuilder(args);

    // Utiliser Serilog
    builder.Host.UseSerilog();

    // Charger la configuration Ocelot
    builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

    // JWT Authentication
    var tokenKey = builder.Configuration["TokenKey"] 
        ?? "REDACTED_JWT_TOKEN_KEY";
    
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

    // Ajouter Ocelot
    builder.Services.AddOcelot(builder.Configuration);

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins("http://localhost:4200", "https://localhost:4200", "http://localhost:8080", "https://localhost:8080")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });

    // Health Checks
    builder.Services.AddHealthChecks();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    app.UseCors();

    // Authentication & Authorization (AVANT Ocelot)
    app.UseAuthentication();
    app.UseAuthorization();

    // Health Check
    app.MapHealthChecks("/health");
    
    // Info endpoint
    app.MapGet("/info", () => new
    {
        service = "ApiGateway",
        version = "1.0.0",
        status = "running",
        timestamp = DateTime.UtcNow
    });

    // Utiliser Ocelot Middleware
    await app.UseOcelot();

    Log.Information("✅ API Gateway started successfully");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ API Gateway terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

