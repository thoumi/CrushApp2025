namespace MediaService.Events;

// Classe de base pour les événements photo (utilisée pour la désérialisation générique)
public class PhotoEvent
{
    public required string PhotoId { get; set; }
    public required string MemberId { get; set; }
    public string? Url { get; set; }
    public string? PublicId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class PhotoUploadedEvent
{
    public required string PhotoId { get; set; }
    public required string MemberId { get; set; }
    public required string Url { get; set; }
    public required string PublicId { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}

public class PhotoApprovedEvent
{
    public required string PhotoId { get; set; }
    public required string MemberId { get; set; }
    public required string Url { get; set; }
    public required string ApprovedBy { get; set; }
    public DateTime ApprovedAt { get; set; } = DateTime.UtcNow;
    public bool IsMain { get; set; }
}

public class PhotoRejectedEvent
{
    public required string PhotoId { get; set; }
    public required string MemberId { get; set; }
    public required string Reason { get; set; }
    public required string RejectedBy { get; set; }
    public DateTime RejectedAt { get; set; } = DateTime.UtcNow;
}

public class PhotoDeletedEvent
{
    public required string PhotoId { get; set; }
    public required string MemberId { get; set; }
    public required string PublicId { get; set; }
    public DateTime DeletedAt { get; set; } = DateTime.UtcNow;
}

