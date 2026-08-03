namespace API.Application.Interfaces;

public interface IMatchingService
{
    Task HandleLikeAddedAsync(string sourceMemberId, string targetMemberId);
    Task HandleLikeRemovedAsync(string sourceMemberId, string targetMemberId);
}
