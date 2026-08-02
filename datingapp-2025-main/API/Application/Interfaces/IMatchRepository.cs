using API.Domain.Entities;

namespace API.Application.Interfaces;

public interface IMatchRepository
{
    Task<Match?> GetMatchAsync(string memberAId, string memberBId);
    void AddMatch(Match match);
    void RemoveMatch(Match match);
}
