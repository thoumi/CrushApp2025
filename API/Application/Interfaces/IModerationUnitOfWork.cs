namespace API.Application.Interfaces;

public interface IModerationUnitOfWork : IMemberUnitOfWork
{
    IAdminRepository AdminRepository { get; }
    IPhotoRepository PhotoRepository { get; }
}
