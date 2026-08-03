using System.Text.Json.Serialization;

namespace API.Domain.Entities;

public class PromptAnswer
{
    public int Id { get; set; }
    public string MemberId { get; set; } = null!;

    [JsonIgnore]
    public Member Member { get; set; } = null!;

    public int PromptId { get; set; }
    public Prompt Prompt { get; set; } = null!;
    public required string Answer { get; set; }
    public int DisplayOrder { get; set; }
}
