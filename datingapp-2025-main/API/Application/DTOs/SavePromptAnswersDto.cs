namespace API.Application.DTOs;

public class PromptAnswerInputDto
{
    public int PromptId { get; set; }
    public required string Answer { get; set; }
}

public class SavePromptAnswersDto
{
    public required List<PromptAnswerInputDto> Answers { get; set; }
}
