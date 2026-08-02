using System;
using API.Application.Helpers;
using API.Application.Interfaces;
using API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Infrastructure.Data;

public class LikesRepository(AppDbContext context) : ILikesRepository
{
    public void AddLike(MemberLike like)
    {
        context.Likes.Add(like);
    }

    public void DeleteLike(MemberLike like)
    {
        context.Likes.Remove(like);
    }

    public async Task<IReadOnlyList<string>> GetCurrentMemberLikeIds(string memberId)
    {
        return await context.Likes
            .Where(x => x.SourceMemberId == memberId)
            .Select(x => x.TargetMemberId)
            .ToListAsync();
    }

    public async Task<MemberLike?> GetMemberLike(string sourceMemberId, string targetMemberId)
    {
        return await context.Likes.FindAsync(sourceMemberId, targetMemberId);
    }

    public async Task<PaginatedResult<Member>> GetMemberLikes(LikesParams likesParams)
    {
        var query = context.Likes.AsQueryable();
        IQueryable<Member> result;

        switch (likesParams.Predicate)
        {
            case "liked":
                result = query
                    .Where(like => like.SourceMemberId == likesParams.MemberId)
                    .Select(like => like.TargetMember);
                break;
            case "likedBy":
                result = query
                    .Where(like => like.TargetMemberId == likesParams.MemberId)
                    .Select(like => like.SourceMember);
                break;
            default: // mutual
                result = context.Matches
                    .Where(m => m.MemberOneId == likesParams.MemberId || m.MemberTwoId == likesParams.MemberId)
                    .Select(m => m.MemberOneId == likesParams.MemberId ? m.MemberTwo : m.MemberOne);
                break;
        }
        
        return await PaginationHelper.CreateAsync(result,
            likesParams.PageNumber, likesParams.PageSize);
    }
}
