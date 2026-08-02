using API.Application.Interfaces;
using API.Domain.Entities;

namespace API.Application.Services;

public class MatchingService(IMatchingUnitOfWork uow) : IMatchingService
{
    public async Task HandleLikeAddedAsync(string sourceMemberId, string targetMemberId)
    {
        var reciprocalLike = await uow.LikesRepository.GetMemberLike(targetMemberId, sourceMemberId);
        if (reciprocalLike == null) return;

        var existingMatch = await uow.MatchRepository.GetMatchAsync(sourceMemberId, targetMemberId);
        if (existingMatch != null) return;

        uow.MatchRepository.AddMatch(Match.Create(sourceMemberId, targetMemberId));
    }

    public async Task HandleLikeRemovedAsync(string sourceMemberId, string targetMemberId)
    {
        var existingMatch = await uow.MatchRepository.GetMatchAsync(sourceMemberId, targetMemberId);
        if (existingMatch != null) uow.MatchRepository.RemoveMatch(existingMatch);
    }
}
