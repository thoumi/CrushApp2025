using System;
using API.Application.DTOs;
using API.Domain.Entities;

namespace API.Application.Interfaces;

public interface IPhotoRepository
{
    Task<IReadOnlyList<PhotoForApprovalDto>> GetUnapprovedPhotos();
    Task<Photo?> GetPhotoById(int id);
    Task<Photo?> GetPhotoByExternalId(string externalPhotoId);
    void RemovePhoto(Photo photo);
}
