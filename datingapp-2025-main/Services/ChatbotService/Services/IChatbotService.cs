namespace ChatbotService.Services;

public interface IChatbotService
{
    Task<string> GetReplyAsync(string prompt);
}

