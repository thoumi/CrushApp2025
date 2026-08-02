namespace API.Application.Events;

/// <summary>
/// Événements publiés par Media Service (Services/MediaService/Events/RabbitMqEventPublisher.cs).
/// Les noms et types de champs doivent rester synchronisés avec le publisher.
/// </summary>
public class PhotoUploadedEvent
{
    public string PhotoId { get; set; } = string.Empty;
    public string MemberId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string PublicId { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

public class PhotoApprovedEvent
{
    public string PhotoId { get; set; } = string.Empty;
    public string MemberId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
    public DateTime ApprovedAt { get; set; }
    public bool IsMain { get; set; }
}

public class PhotoRejectedEvent
{
    public string PhotoId { get; set; } = string.Empty;
    public string MemberId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string RejectedBy { get; set; } = string.Empty;
    public DateTime RejectedAt { get; set; }
}
