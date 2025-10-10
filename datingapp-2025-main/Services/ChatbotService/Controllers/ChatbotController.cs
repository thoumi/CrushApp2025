using ChatbotService.Models;
using ChatbotService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace ChatbotService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatbotController : ControllerBase
{
    private readonly IChatbotService _chatbotService;
    private readonly IMemoryCache _memoryCache;
    private readonly IConfiguration _config;
    private readonly ILogger<ChatbotController> _logger;

    public ChatbotController(
        IChatbotService chatbotService, 
        IMemoryCache memoryCache, 
        IConfiguration config,
        ILogger<ChatbotController> logger)
    {
        _chatbotService = chatbotService;
        _memoryCache = memoryCache;
        _config = config;
        _logger = logger;
    }

    [HttpPost("ask")]
    public async Task<ActionResult<ChatResponse>> Ask([FromBody] ChatRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Unauthorized chatbot request");
            return Unauthorized();
        }

        _logger.LogInformation("Chatbot request from user: {UserId}", userId);

        // 1. Récupérer l'historique de la conversation depuis le cache
        var history = _memoryCache.Get<List<string>>(userId) ?? new List<string>();
        
        // 2. Construire le prompt avec un rôle et l'historique (Prompt Engineering)
        var systemPrompt = _config["Chatbot:SystemPrompt"] ?? 
            "Tu es Crush Helper, un assistant virtuel amical pour une application de rencontres.";
        var historyExpirationMinutes = int.TryParse(
            _config["Chatbot:HistoryExpirationMinutes"], out var minutes) ? minutes : 30;
        
        var historyPrompt = string.Join("\n", history);
        var userPrompt = $"Utilisateur: {request.Prompt}";

        var fullPrompt = $"{systemPrompt}\n{historyPrompt}\n{userPrompt}\nCrush Helper:";

        // 3. Obtenir la réponse d'Ollama
        var botResponse = await _chatbotService.GetReplyAsync(fullPrompt);

        // 4. Mettre à jour l'historique avec le tour de conversation actuel
        history.Add(userPrompt);
        history.Add($"Crush Helper: {botResponse}");
        
        // Limiter l'historique aux 10 derniers échanges
        if (history.Count > 20)
        {
            history = history.Skip(history.Count - 20).ToList();
        }
        
        _memoryCache.Set(userId, history, TimeSpan.FromMinutes(historyExpirationMinutes));

        _logger.LogInformation("Chatbot response sent to user: {UserId}", userId);

        return Ok(new ChatResponse { Response = botResponse });
    }

    [HttpDelete("history")]
    public ActionResult ClearHistory()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        _memoryCache.Remove(userId);
        _logger.LogInformation("Chat history cleared for user: {UserId}", userId);

        return Ok(new { message = "Historique effacé" });
    }
}

