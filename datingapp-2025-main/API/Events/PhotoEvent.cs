namespace API.Events;

/// <summary>
/// Événement publié par le Media Service lors d'opérations sur les photos
/// </summary>
public class PhotoEvent
{
    public string EventType { get; set; } = string.Empty; // "photo.uploaded", "photo.approved", "photo.rejected", "photo.deleted"
    public int PhotoId { get; set; }
    public int UserId { get; set; }
    public string? PhotoUrl { get; set; }
    public bool IsMain { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, string> Metadata { get; set; } = new();
}


