using API.Application.Interfaces;
using API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Infrastructure.Data;

public class MatchRepository(AppDbContext context) : IMatchRepository
{
    public async Task<Match?> GetMatchAsync(string memberAId, string memberBId)
    {
        var (first, second) = Match.OrderIds(memberAId, memberBId);

        return await context.Matches
            .FirstOrDefaultAsync(m => m.MemberOneId == first && m.MemberTwoId == second);
    }

    public void AddMatch(Match match)
    {
        context.Matches.Add(match);
    }

    public void RemoveMatch(Match match)
    {
        context.Matches.Remove(match);
    }
}
