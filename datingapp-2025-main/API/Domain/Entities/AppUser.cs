using Microsoft.AspNetCore.Identity;

namespace API.Domain.Entities;


// je le laisse en Domain à voir si je l'isole dans Infrastructure/Identity/.
public class AppUser : IdentityUser
{
    public required string DisplayName { get; set; }
    public string? ImageUrl { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

    // Nav property
    public Member Member { get; set; } = null!;
}
