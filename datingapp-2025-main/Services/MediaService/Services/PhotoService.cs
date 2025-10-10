using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using MediaService.Models;
using Microsoft.Extensions.Options;

namespace MediaService.Services;

public class PhotoService : IPhotoService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<PhotoService> _logger;

    public PhotoService(IOptions<CloudinarySettings> config, ILogger<PhotoService> logger)
    {
        _logger = logger;

        var account = new Account(
            config.Value.CloudName,
            config.Value.ApiKey,
            config.Value.ApiSecret
        );

        _cloudinary = new Cloudinary(account);
    }

    public async Task<ImageUploadResult> AddPhotoAsync(IFormFile file)
    {
        if (file.Length <= 0)
        {
            _logger.LogWarning("Empty file provided for upload");
            return new ImageUploadResult { Error = new Error { Message = "File is empty" } };
        }

        try
        {
            _logger.LogInformation("Uploading photo to Cloudinary, size: {Size} bytes", file.Length);

            await using var stream = file.OpenReadStream();
            
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Transformation = new Transformation()
                    .Height(500)
                    .Width(500)
                    .Crop("fill")
                    .Gravity("face"),
                Folder = "crushapp"
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
            {
                _logger.LogError("Cloudinary upload error: {Error}", result.Error.Message);
            }
            else
            {
                _logger.LogInformation("✅ Photo uploaded successfully: {PublicId}", result.PublicId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Exception during photo upload");
            return new ImageUploadResult { Error = new Error { Message = ex.Message } };
        }
    }

    public async Task<DeletionResult> DeletePhotoAsync(string publicId)
    {
        try
        {
            _logger.LogInformation("Deleting photo from Cloudinary: {PublicId}", publicId);

            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);

            if (result.Error != null)
            {
                _logger.LogError("Cloudinary deletion error: {Error}", result.Error.Message);
            }
            else
            {
                _logger.LogInformation("✅ Photo deleted successfully: {PublicId}", publicId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Exception during photo deletion");
            return new DeletionResult { Error = new CloudinaryDotNet.Actions.Error { Message = ex.Message } };
        }
    }
}

