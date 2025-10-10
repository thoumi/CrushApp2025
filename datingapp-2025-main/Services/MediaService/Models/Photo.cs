using System.ComponentModel.DataAnnotations;

namespace MediaService.Models;

public class Photo
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public required string Url { get; set; }
    
    public required string PublicId { get; set; }
    
    public bool IsApproved { get; set; } = false;
    
    public required string MemberId { get; set; }
    
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ApprovedAt { get; set; }
    
    public string? ApprovedBy { get; set; }
    
    public bool IsMain { get; set; } = false;
}

