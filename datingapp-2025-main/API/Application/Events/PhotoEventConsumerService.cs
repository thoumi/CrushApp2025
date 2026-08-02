using API.Application.Interfaces;
using API.Domain.Entities;

namespace API.Application.Events;

/// <summary>
/// Background service qui consomme les événements photo depuis RabbitMQ
/// et met à jour la base de données locale (Core API)
/// </summary>
public class PhotoEventConsumerService : BackgroundService
{
    private readonly ILogger<PhotoEventConsumerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventConsumer _eventConsumer;

    public PhotoEventConsumerService(
        ILogger<PhotoEventConsumerService> logger,
        IServiceProvider serviceProvider,
        IEventConsumer eventConsumer)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _eventConsumer = eventConsumer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 PhotoEventConsumerService started");

        // Consommer les événements photo.uploaded
        await _eventConsumer.ConsumeAsync<PhotoUploadedEvent>(
            "photo.uploaded",
            async (photoEvent) => await HandlePhotoUploadedAsync(photoEvent),
            stoppingToken);

        // Consommer les événements photo.approved
        await _eventConsumer.ConsumeAsync<PhotoApprovedEvent>(
            "photo.approved",
            async (photoEvent) => await HandlePhotoApprovedAsync(photoEvent),
            stoppingToken);

        // Consommer les événements photo.rejected
        await _eventConsumer.ConsumeAsync<PhotoRejectedEvent>(
            "photo.rejected",
            async (photoEvent) => await HandlePhotoRejectedAsync(photoEvent),
            stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandlePhotoUploadedAsync(PhotoUploadedEvent photoEvent)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        try
        {
            _logger.LogInformation("📸 Processing photo.uploaded event: PhotoId={PhotoId}, MemberId={MemberId}",
                photoEvent.PhotoId, photoEvent.MemberId);

            var member = await unitOfWork.MemberRepository.GetMemberForUpdate(photoEvent.MemberId);
            if (member == null)
            {
                _logger.LogWarning("⚠️ Member not found: {MemberId}", photoEvent.MemberId);
                return;
            }

            var existingPhoto = member.Photos?.FirstOrDefault(p => p.ExternalPhotoId == photoEvent.PhotoId);
            if (existingPhoto == null)
            {
                member.Photos ??= new List<Photo>();
                member.Photos.Add(new Photo
                {
                    ExternalPhotoId = photoEvent.PhotoId,
                    Url = photoEvent.Url,
                    IsApproved = false,
                    PublicId = photoEvent.PublicId,
                    MemberId = member.Id
                });

                await unitOfWork.Complete();
                _logger.LogInformation("✅ Photo added to member profile: PhotoId={PhotoId}", photoEvent.PhotoId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error handling photo.uploaded event");
        }
    }

    private async Task HandlePhotoApprovedAsync(PhotoApprovedEvent photoEvent)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        try
        {
            _logger.LogInformation("✅ Processing photo.approved event: PhotoId={PhotoId}", photoEvent.PhotoId);

            var photo = await unitOfWork.PhotoRepository.GetPhotoByExternalId(photoEvent.PhotoId);
            if (photo != null)
            {
                photo.IsApproved = true;
                photo.IsMain = photoEvent.IsMain;
                await unitOfWork.Complete();
                _logger.LogInformation("✅ Photo approved in Core API: PhotoId={PhotoId}", photoEvent.PhotoId);
            }
            else
            {
                _logger.LogWarning("⚠️ Approved photo not found in Core API read model: {PhotoId}", photoEvent.PhotoId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error handling photo.approved event");
        }
    }

    private async Task HandlePhotoRejectedAsync(PhotoRejectedEvent photoEvent)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        try
        {
            _logger.LogInformation("❌ Processing photo.rejected event: PhotoId={PhotoId}", photoEvent.PhotoId);

            var photo = await unitOfWork.PhotoRepository.GetPhotoByExternalId(photoEvent.PhotoId);
            if (photo != null)
            {
                unitOfWork.PhotoRepository.RemovePhoto(photo);
                await unitOfWork.Complete();
                _logger.LogInformation("✅ Photo removed from Core API: PhotoId={PhotoId}", photoEvent.PhotoId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error handling photo.rejected event");
        }
    }
}

