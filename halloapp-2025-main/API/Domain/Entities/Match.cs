namespace API.Domain.Entities;

public class Match
{
    public Guid Id { get; set; }
    public required string MemberOneId { get; set; }
    public Member MemberOne { get; set; } = null!;
    public required string MemberTwoId { get; set; }
    public Member MemberTwo { get; set; } = null!;
    public DateTime MatchedOn { get; set; } = DateTime.UtcNow;

    public static Match Create(string memberAId, string memberBId)
    {
        var (first, second) = OrderIds(memberAId, memberBId);

        return new Match
        {
            Id = Guid.NewGuid(),
            MemberOneId = first,
            MemberTwoId = second,
            MatchedOn = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Ordonne deux ids de membre de façon déterministe, pour qu'une paire (A,B)
    /// et (B,A) désigne toujours la même ligne en base (contrainte d'unicité).
    /// </summary>
    public static (string First, string Second) OrderIds(string memberAId, string memberBId)
    {
        if (memberAId == memberBId)
            throw new ArgumentException("Un membre ne peut pas matcher avec lui-même.");

        return string.CompareOrdinal(memberAId, memberBId) <= 0
            ? (memberAId, memberBId)
            : (memberBId, memberAId);
    }
}
