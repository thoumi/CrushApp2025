using API.Application.Interfaces;
using API.Domain.Entities;

namespace API.Application.Services;

public class DailySelectionService(IDailySelectionUnitOfWork uow) : IDailySelectionService
{
    public async Task<IReadOnlyList<Member>> GetDailySelectionAsync(string currentMemberId, int count)
    {
        var likedIds = await uow.LikesRepository.GetCurrentMemberLikeIds(currentMemberId);

        var candidateIds = await uow.MemberRepository.GetCandidateMemberIdsAsync(currentMemberId, likedIds);

        // Déterministe pour la journée : même seed toute la journée, change le lendemain.
        // Évite d'avoir besoin d'un job de recommandation précalculé pour ce volume de candidats.
        var seed = StableHash(currentMemberId + DateOnly.FromDateTime(DateTime.UtcNow));
        var rng = new Random(seed);

        var selectedIds = candidateIds
            .OrderBy(_ => rng.Next())
            .Take(count)
            .ToList();

        var members = await uow.MemberRepository.GetMembersByIdsAsync(selectedIds);

        return selectedIds
            .Select(id => members.First(m => m.Id == id))
            .ToList();
    }

    private static int StableHash(string value)
    {
        unchecked
        {
            var hash = 17;
            foreach (var c in value) hash = hash * 31 + c;
            return hash;
        }
    }
}
