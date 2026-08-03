using System;
using API.Application.Helpers;
using API.Domain.Entities;

namespace API.Application.Interfaces;

public interface IMemberRepository
{
    void Update(Member member);
    Task<PaginatedResult<Member>> GetMembersAsync(MemberParams memberParams);
    Task<Member?> GetMemberByIdAsync(string id);
    Task<IReadOnlyList<Photo>> GetPhotosForMemberAsync(string memberId, bool isCurrentUser);
    Task<Member?> GetMemberForUpdate(string id);
    Task<IReadOnlyList<Prompt>> GetPromptBankAsync();
    Task<IReadOnlyList<string>> GetCandidateMemberIdsAsync(string currentMemberId, IReadOnlyCollection<string> excludedIds);
    Task<IReadOnlyList<Member>> GetMembersByIdsAsync(IReadOnlyCollection<string> ids);
}
