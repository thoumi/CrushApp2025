using ChatbotService.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace ChatbotService.Services;

public class OllamaChatbotService : IChatbotService
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;
    private readonly ILogger<OllamaChatbotService> _logger;

    public OllamaChatbotService(HttpClient httpClient, OllamaOptions options, ILogger<OllamaChatbotService> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(60);
    }

    public async Task<string> GetReplyAsync(string prompt)
    {
        try
        {
            _logger.LogInformation("Sending prompt to Ollama: {Model}", _options.Model);

            var request = new
            {
                model = _options.Model,
                prompt = prompt,
                stream = false,
                options = new
                {
                    temperature = _options.Temperature,
                    num_predict = _options.MaxTokens
                }
            };

            var response = await _httpClient.PostAsJsonAsync("/api/generate", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            var reply = result.GetProperty("response").GetString() ?? "Désolé, je n'ai pas pu générer de réponse.";

            _logger.LogInformation("Received response from Ollama, length: {Length}", reply.Length);

            return reply;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Ollama API");
            return "Désolé, une erreur s'est produite. Le service de chatbot est peut-être indisponible.";
        }
    }
}

