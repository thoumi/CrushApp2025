using System;
using System.Text.Json.Serialization;

namespace API.Domain.Entities;

public class Photo
{
    public int Id { get; set; }
    public required string Url { get; set; }
    public string? PublicId { get; set; }
    public bool IsApproved { get; set; }

    // Id of the corresponding photo in MediaService's own database (its source of truth)
    public string? ExternalPhotoId { get; set; }

    // Navigation property
    [JsonIgnore]
    public Member Member { get; set; } = null!;
    public string MemberId { get; set; } = null!;
    public bool IsMain { get; set; }
}
