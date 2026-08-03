using System;
using API.Application.Helpers;
using API.Application.Interfaces;
using API.Domain.Entities;
using API.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace API.Infrastructure.Data;

public class MemberRepository(AppDbContext context) : IMemberRepository
{
    public async Task<Member?> GetMemberByIdAsync(string id)
    {
        return await context.Members
            .Include(x => x.PromptAnswers).ThenInclude(x => x.Prompt)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Member?> GetMemberForUpdate(string id)
    {
        return await context.Members
            .Include(x => x.User)
            .Include(x => x.Photos)
            .Include(x => x.PromptAnswers)
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IReadOnlyList<Prompt>> GetPromptBankAsync()
    {
        return await context.Prompts.OrderBy(x => x.Id).ToListAsync();
    }

    public async Task<IReadOnlyList<string>> GetCandidateMemberIdsAsync(string currentMemberId, IReadOnlyCollection<string> excludedIds)
    {
        return await context.Members
            .Where(m => m.Id != currentMemberId && !excludedIds.Contains(m.Id))
            .Select(m => m.Id)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Member>> GetMembersByIdsAsync(IReadOnlyCollection<string> ids)
    {
        return await context.Members
            .Include(x => x.PromptAnswers).ThenInclude(x => x.Prompt)
            .Where(m => ids.Contains(m.Id))
            .ToListAsync();
    }

    public async Task<PaginatedResult<Member>> GetMembersAsync(MemberParams memberParams)
    {
        var query = context.Members
            .Include(x => x.PromptAnswers).ThenInclude(x => x.Prompt)
            .AsQueryable();

        query = query.Where(x => x.Id != memberParams.CurrentMemberId);

        if (memberParams.Gender != null)
        {
            query = query.Where(x => x.Gender == memberParams.Gender);
        }

        var ageRange = AgeRange.Create(memberParams.MinAge, memberParams.MaxAge);
        var (minDob, maxDob) = ageRange.ToDateOfBirthBounds(DateOnly.FromDateTime(DateTime.Today));

        query = query.Where(x => x.DateOfBirth >= minDob && x.DateOfBirth <= maxDob);

        query = memberParams.OrderBy switch
        {
            "created" => query.OrderByDescending(x => x.Created),
            _ => query.OrderByDescending(x => x.LastActive)
        };

        return await PaginationHelper.CreateAsync(query,
                memberParams.PageNumber, memberParams.PageSize);
    }

    public async Task<IReadOnlyList<Photo>> GetPhotosForMemberAsync(string memberId, bool isCurrentUser)
    {
        var query = context.Members
            .Where(x => x.Id == memberId)
            .SelectMany(x => x.Photos);

        if (isCurrentUser) query = query.IgnoreQueryFilters();

        return await query.ToListAsync();
    }

    public void Update(Member member)
    {
        context.Entry(member).State = EntityState.Modified;
    }
}
