using System;

namespace API.DTOs;

public class UsersManageDto
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public required string DisplayName { get; set; }
    public bool IsLockedOut { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
}
