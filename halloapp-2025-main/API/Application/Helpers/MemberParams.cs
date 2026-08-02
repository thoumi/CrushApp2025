using System;
using API.Domain.ValueObjects;

namespace API.Application.Helpers;

public class MemberParams : PagingParams
{
    public string? Gender { get; set; }
    public string? CurrentMemberId { get; set; }
    public int MinAge { get; set; } = AgeRange.MinimumAllowedAge;
    public int MaxAge { get; set; } = AgeRange.MaximumAllowedAge;
    public string OrderBy { get; set; } = "lastActive";
    public string? Roles { get; set; }
    public bool? IsLockedOut { get; set; }
    public string? SearchTerm { get; set; }
}
