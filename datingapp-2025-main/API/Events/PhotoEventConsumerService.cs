using API.Interfaces;

namespace API.Events;

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
        await _eventConsumer.ConsumeAsync<PhotoEvent>(
            "photo.uploaded",
            async (photoEvent) => await HandlePhotoUploadedAsync(photoEvent),
            stoppingToken);

        // Consommer les événements photo.approved
        await _eventConsumer.ConsumeAsync<PhotoEvent>(
            "photo.approved",
            async (photoEvent) => await HandlePhotoApprovedAsync(photoEvent),
            stoppingToken);

        // Consommer les événements photo.rejected
        await _eventConsumer.ConsumeAsync<PhotoEvent>(
            "photo.rejected",
            async (photoEvent) => await HandlePhotoRejectedAsync(photoEvent),
            stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandlePhotoUploadedAsync(PhotoEvent photoEvent)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        try
        {
            _logger.LogInformation("📸 Processing photo.uploaded event: PhotoId={PhotoId}, UserId={UserId}",
                photoEvent.PhotoId, photoEvent.UserId);

            // Récupérer le membre
            var member = await unitOfWork.MemberRepository.GetMemberForUpdate(photoEvent.UserId.ToString());
            if (member == null)
            {
                _logger.LogWarning("⚠️ Member not found: {UserId}", photoEvent.UserId);
                return;
            }

            // Ajouter la photo si elle n'existe pas déjà
            var existingPhoto = member.Photos?.FirstOrDefault(p => p.Id == photoEvent.PhotoId);
            if (existingPhoto == null && photoEvent.PhotoUrl != null)
            {
                member.Photos ??= new List<Entities.Photo>();
                member.Photos.Add(new Entities.Photo
                {
                    Id = photoEvent.PhotoId,
                    Url = photoEvent.PhotoUrl,
                    IsApproved = false,
                    IsMain = photoEvent.IsMain,
                    PublicId = photoEvent.Metadata.GetValueOrDefault("PublicId"),
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

    private async Task HandlePhotoApprovedAsync(PhotoEvent photoEvent)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        try
        {
            _logger.LogInformation("✅ Processing photo.approved event: PhotoId={PhotoId}", photoEvent.PhotoId);

            var photo = await unitOfWork.PhotoRepository.GetPhotoById(photoEvent.PhotoId);
            if (photo != null)
            {
                photo.IsApproved = true;
                await unitOfWork.Complete();
                _logger.LogInformation("✅ Photo approved in Core API: PhotoId={PhotoId}", photoEvent.PhotoId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error handling photo.approved event");
        }
    }

    private async Task HandlePhotoRejectedAsync(PhotoEvent photoEvent)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        try
        {
            _logger.LogInformation("❌ Processing photo.rejected event: PhotoId={PhotoId}", photoEvent.PhotoId);

            var photo = await unitOfWork.PhotoRepository.GetPhotoById(photoEvent.PhotoId);
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

