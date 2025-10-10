using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace MediaService.Services;

public interface IPhotoService
{
    Task<ImageUploadResult> AddPhotoAsync(IFormFile file);
    Task<DeletionResult> DeletePhotoAsync(string publicId);
}

