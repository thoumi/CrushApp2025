using API.Application.DTOs;
using API.Application.Helpers;
using API.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System;

namespace API.Application.Interfaces;

public interface IAdminRepository
{
    public Task<PaginatedResult<UsersManageDto>> GetUsersWithRoles([FromQuery] MemberParams memberParams);
}
