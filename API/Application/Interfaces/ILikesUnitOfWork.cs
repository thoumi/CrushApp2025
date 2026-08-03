namespace API.Application.Interfaces;

public interface ILikesUnitOfWork : IUnitOfWorkBase
{
    ILikesRepository LikesRepository { get; }
}
