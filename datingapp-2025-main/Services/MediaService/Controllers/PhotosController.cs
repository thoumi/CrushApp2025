using MediaService.Data;
using MediaService.Events;
using MediaService.Models;
using MediaService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MediaService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PhotosController : ControllerBase
{
    private readonly MediaDbContext _context;
    private readonly IPhotoService _photoService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<PhotosController> _logger;

    public PhotosController(
        MediaDbContext context,
        IPhotoService photoService,
        IEventPublisher eventPublisher,
        ILogger<PhotosController> logger)
    {
        _context = context;
        _photoService = photoService;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    // GET: api/photos/member/{memberId}
    [HttpGet("member/{memberId}")]
    public async Task<ActionResult<IEnumerable<Photo>>> GetMemberPhotos(string memberId)
    {
        var photos = await _context.Photos
            .Where(p => p.MemberId == memberId && p.IsApproved)
            .OrderByDescending(p => p.IsMain)
            .ThenByDescending(p => p.UploadedAt)
            .ToListAsync();

        return Ok(photos);
    }

    // GET: api/photos/pending (Admin/Moderator only)
    [HttpGet("pending")]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<ActionResult<IEnumerable<Photo>>> GetPendingPhotos()
    {
        var photos = await _context.Photos
            .Where(p => !p.IsApproved)
            .OrderBy(p => p.UploadedAt)
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} pending photos", photos.Count);
        return Ok(photos);
    }

    // POST: api/photos
    [HttpPost]
    public async Task<ActionResult<Photo>> UploadPhoto([FromForm] IFormFile file)
    {
        var memberId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(memberId))
        {
            return Unauthorized();
        }

        _logger.LogInformation("User {MemberId} uploading photo", memberId);

        // Upload to Cloudinary
        var result = await _photoService.AddPhotoAsync(file);

        if (result.Error != null)
        {
            _logger.LogError("Photo upload failed: {Error}", result.Error.Message);
            return BadRequest(result.Error.Message);
        }

        // Create Photo entity
        var photo = new Photo
        {
            Url = result.SecureUrl.ToString(),
            PublicId = result.PublicId,
            MemberId = memberId,
            IsApproved = false  // Requires moderation
        };

        _context.Photos.Add(photo);
        await _context.SaveChangesAsync();

        _logger.LogInformation("✅ Photo saved to database: {PhotoId}", photo.Id);

        // Publish event
        await _eventPublisher.PublishAsync("photo.uploaded", new PhotoUploadedEvent
        {
            PhotoId = photo.Id,
            MemberId = photo.MemberId,
            Url = photo.Url,
            PublicId = photo.PublicId,
            UploadedAt = photo.UploadedAt
        });

        _logger.LogInformation("📨 photo.uploaded event published");

        return CreatedAtAction(nameof(GetPhoto), new { id = photo.Id }, photo);
    }

    // GET: api/photos/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Photo>> GetPhoto(string id)
    {
        var photo = await _context.Photos.FindAsync(id);
        
        if (photo == null)
        {
            return NotFound();
        }

        return Ok(photo);
    }

    // PUT: api/photos/{id}/approve (Admin/Moderator only)
    [HttpPut("{id}/approve")]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<ActionResult> ApprovePhoto(string id)
    {
        var approvedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(approvedBy))
        {
            return Unauthorized();
        }

        var photo = await _context.Photos.FindAsync(id);
        if (photo == null)
        {
            return NotFound();
        }

        photo.IsApproved = true;
        photo.ApprovedAt = DateTime.UtcNow;
        photo.ApprovedBy = approvedBy;

        await _context.SaveChangesAsync();

        _logger.LogInformation("✅ Photo {PhotoId} approved by {ApprovedBy}", id, approvedBy);

        // Publish event
        await _eventPublisher.PublishAsync("photo.approved", new PhotoApprovedEvent
        {
            PhotoId = photo.Id,
            MemberId = photo.MemberId,
            Url = photo.Url,
            ApprovedBy = approvedBy,
            ApprovedAt = photo.ApprovedAt.Value,
            IsMain = photo.IsMain
        });

        _logger.LogInformation("📨 photo.approved event published");

        return NoContent();
    }

    // PUT: api/photos/{id}/reject (Admin/Moderator only)
    [HttpPut("{id}/reject")]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<ActionResult> RejectPhoto(string id, [FromBody] string reason)
    {
        var rejectedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(rejectedBy))
        {
            return Unauthorized();
        }

        var photo = await _context.Photos.FindAsync(id);
        if (photo == null)
        {
            return NotFound();
        }

        var memberId = photo.MemberId;
        var publicId = photo.PublicId;

        // Delete from Cloudinary
        await _photoService.DeletePhotoAsync(photo.PublicId);

        // Delete from database
        _context.Photos.Remove(photo);
        await _context.SaveChangesAsync();

        _logger.LogInformation("❌ Photo {PhotoId} rejected by {RejectedBy}", id, rejectedBy);

        // Publish event
        await _eventPublisher.PublishAsync("photo.rejected", new PhotoRejectedEvent
        {
            PhotoId = id,
            MemberId = memberId,
            Reason = reason,
            RejectedBy = rejectedBy,
            RejectedAt = DateTime.UtcNow
        });

        _logger.LogInformation("📨 photo.rejected event published");

        return NoContent();
    }

    // DELETE: api/photos/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePhoto(string id)
    {
        var memberId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(memberId))
        {
            return Unauthorized();
        }

        var photo = await _context.Photos.FindAsync(id);
        if (photo == null)
        {
            return NotFound();
        }

        // Check ownership
        if (photo.MemberId != memberId)
        {
            return Forbid();
        }

        // Can't delete main photo
        if (photo.IsMain)
        {
            return BadRequest("Cannot delete main photo");
        }

        // Delete from Cloudinary
        var result = await _photoService.DeletePhotoAsync(photo.PublicId);
        if (result.Error != null)
        {
            return BadRequest(result.Error);
        }

        // Delete from database
        _context.Photos.Remove(photo);
        await _context.SaveChangesAsync();

        _logger.LogInformation("🗑️ Photo {PhotoId} deleted by owner", id);

        // Publish event
        await _eventPublisher.PublishAsync("photo.deleted", new PhotoDeletedEvent
        {
            PhotoId = id,
            MemberId = memberId,
            PublicId = photo.PublicId,
            DeletedAt = DateTime.UtcNow
        });

        return NoContent();
    }

    // PUT: api/photos/{id}/set-main
    [HttpPut("{id}/set-main")]
    public async Task<ActionResult> SetMainPhoto(string id)
    {
        var memberId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(memberId))
        {
            return Unauthorized();
        }

        var photo = await _context.Photos.FindAsync(id);
        if (photo == null || photo.MemberId != memberId)
        {
            return NotFound();
        }

        if (!photo.IsApproved)
        {
            return BadRequest("Photo must be approved first");
        }

        if (photo.IsMain)
        {
            return BadRequest("This is already your main photo");
        }

        // Remove current main photo
        var currentMain = await _context.Photos
            .FirstOrDefaultAsync(p => p.MemberId == memberId && p.IsMain);
        
        if (currentMain != null)
        {
            currentMain.IsMain = false;
        }

        photo.IsMain = true;
        await _context.SaveChangesAsync();

        _logger.LogInformation("📌 Photo {PhotoId} set as main for member {MemberId}", id, memberId);

        return NoContent();
    }
}

