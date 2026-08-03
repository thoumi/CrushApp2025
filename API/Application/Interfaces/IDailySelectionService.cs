using API.Domain.Entities;

namespace API.Application.Interfaces;

public interface IDailySelectionService
{
    Task<IReadOnlyList<Member>> GetDailySelectionAsync(string currentMemberId, int count);
}
