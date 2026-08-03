namespace API.Application.Interfaces;

public interface IDailySelectionUnitOfWork : IMemberUnitOfWork
{
    ILikesRepository LikesRepository { get; }
}
