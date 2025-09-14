using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Chatbot.Core.Models;
using Chatbot.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Runtime.ConstrainedExecution;

namespace API.Controllers
{
    public class ChatbotController : BaseApiController
    {
        private readonly IChatbotService _chatbotService;
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _config;



        public ChatbotController(IChatbotService chatbotService, IMemoryCache memoryCache, IConfiguration config)
        {
            _chatbotService = chatbotService;
            _memoryCache = memoryCache;
            _config = config;
        }

        [HttpPost("ask")]
        public async Task<ActionResult<ChatResponse>> Ask([FromBody] ChatRequest request)
        {
            var user = User.GetMemberId();
            if (string.IsNullOrEmpty(user)) return Unauthorized();
            // 1. Récupérer l'historique de la conversation depuis le cache
            var history = _memoryCache.Get<List<string>>(user) ?? new List<string>();
            // 2. Construire le prompt avec un rôle et l'historique (Prompt Engineering)
            var systemPrompt = _config["Chatbot:SystemPrompt"];
            var historyExpirationMinutes = int.TryParse(_config["Chatbot:HistoryExpirationMinutes"], out var minutes) ? minutes : 30;
            var historyPrompt = string.Join("\n", history);
            var userPrompt = $"Utilisateur: {request.Prompt}";

            var fullPrompt = $"{systemPrompt}\n{historyPrompt}\n{userPrompt}\nCrush Helper:";

            // 3. Obtenir la réponse d'Ollama
            var botResponse = await _chatbotService.GetReplyAsync(fullPrompt);

            // 4. Mettre à jour l'historique avec le tour de conversation actuel
            history.Add(userPrompt);
            history.Add($"Crush Helper: {botResponse}");
            _memoryCache.Set(user, history, TimeSpan.FromMinutes(historyExpirationMinutes)); // Conserver l'historique 30 min

            return Ok(new { response = botResponse });
        }
    }
}
