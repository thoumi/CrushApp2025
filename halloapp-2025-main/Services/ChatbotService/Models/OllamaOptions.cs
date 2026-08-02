namespace ChatbotService.Models;

public class OllamaOptions
{
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string Model { get; set; } = "phi3";
    public int MaxTokens { get; set; } = 500;
    public double Temperature { get; set; } = 0.7;
}


