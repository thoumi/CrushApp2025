namespace API.Application.Interfaces;

public interface IMatchingUnitOfWork : ILikesUnitOfWork
{
    IMatchRepository MatchRepository { get; }
}
