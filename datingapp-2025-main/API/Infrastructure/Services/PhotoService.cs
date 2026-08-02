using System;
using API.Application.Helpers;
using API.Application.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace API.Infrastructure.Services;

public class PhotoService : IPhotoService
{
    private readonly Lazy<Cloudinary> _cloudinary;
    public PhotoService(IOptions<CloudinarySettings> config)
    {
        _cloudinary = new Lazy<Cloudinary>(() =>
        {
            var account = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret);

            return new Cloudinary(account);
        });
    }

    public async Task<DeletionResult> DeletePhotoAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);

        return await _cloudinary.Value.DestroyAsync(deleteParams);
    }

    public async Task<ImageUploadResult> UploadPhotoAsync(IFormFile file)
    {
        var uploadResult = new ImageUploadResult();

        if (file.Length > 0)
        {
            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Transformation = new Transformation().Height(500).Width(500).Crop("fill").Gravity("face"),
                Folder = "da-ang20"
            };
            uploadResult = await _cloudinary.Value.UploadAsync(uploadParams);
        }

        return uploadResult;
    }
}
