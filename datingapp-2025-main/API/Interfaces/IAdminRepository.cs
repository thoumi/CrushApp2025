using API.DTOs;
using API.Entities;
using API.Helpers;
using Microsoft.AspNetCore.Mvc;
using System;

namespace API.Interfaces;

public interface IAdminRepository
{
    public Task<PaginatedResult<UsersManageDto>> GetUsersWithRoles([FromQuery] MemberParams memberParams);
}
