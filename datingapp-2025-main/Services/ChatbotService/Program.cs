using ChatbotService.Models;
using ChatbotService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

// Configuration de Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.WithProperty("Service", "ChatbotService")
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Seq(Environment.GetEnvironmentVariable("SEQ_URL") ?? "http://seq:5341")
    .CreateLogger();

try
{
    Log.Information("🚀 Starting ChatbotService...");

    var builder = WebApplication.CreateBuilder(args);

    // Utiliser Serilog
    builder.Host.UseSerilog();

    // Add services to the container
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Memory Cache pour l'historique des conversations
    builder.Services.AddMemoryCache();

    // Configuration Ollama
    var ollamaOptions = builder.Configuration.GetSection("Ollama").Get<OllamaOptions>() 
        ?? new OllamaOptions();
    
    builder.Services.AddSingleton(ollamaOptions);

    // Chatbot Service avec HttpClient
    builder.Services.AddHttpClient<IChatbotService, OllamaChatbotService>()
        .SetHandlerLifetime(TimeSpan.FromMinutes(5));

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
    builder.Services.AddHealthChecks();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    
    // Health Check Endpoints
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/ready");
    
    // Info endpoint
    app.MapGet("/info", () => new
    {
        service = "ChatbotService",
        version = "1.0.0",
        status = "running",
        timestamp = DateTime.UtcNow
    });

    Log.Information("✅ ChatbotService started successfully on port {Port}", 
        app.Configuration["ASPNETCORE_URLS"] ?? "unknown");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ ChatbotService terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

