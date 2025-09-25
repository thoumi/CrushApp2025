using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;

namespace API.Data;

public class AdminRepository(AppDbContext context, UserManager<AppUser> userManager) : IAdminRepository
{
    public async Task<PaginatedResult<UsersManageDto>> GetUsersWithRoles([FromQuery] MemberParams memberParams)
    {
        var members = new List<UsersManageDto>();
        var query = from u in context.Users
                    select new UsersManageDto
                    {
                        Id = u.Id,
                        Email = u.Email,
                        DisplayName = u.DisplayName,
                        Roles = (from ur in context.UserRoles
                                 join r in context.Roles on ur.RoleId equals r.Id
                                 where ur.UserId == u.Id
                                 select r.Name).ToList(),
                        IsLockedOut = u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.Now
                    };

        if (!string.IsNullOrEmpty(memberParams.Roles))
        {
            var rolesToFilter = memberParams.Roles.Split(",", StringSplitOptions.RemoveEmptyEntries)
                                                    .Select(r => r.Trim())
                                                    .ToArray();
            
            // Filtrer les utilisateurs qui ont au moins un des rôles spécifiés
            var userIdsWithRoles = (from ur in context.UserRoles
                                   join r in context.Roles on ur.RoleId equals r.Id
                                   where rolesToFilter.Contains(r.Name)
                                   select ur.UserId).Distinct();
            
            query = query.Where(u => userIdsWithRoles.Contains(u.Id));
        }
        if (memberParams.IsLockedOut.HasValue)
        {
            if (memberParams.IsLockedOut.Value)
            {
                query = query.Where(u => u.IsLockedOut);
            }
            else
            {
                query = query.Where(u => !u.IsLockedOut);
            }
        }

        if (!string.IsNullOrEmpty(memberParams.SearchTerm))
        {
            var searchTerm = memberParams.SearchTerm.ToLower();
            query = query.Where(u => u.DisplayName.ToLower().Contains(searchTerm) || u.Email.ToLower().Contains(searchTerm));
        }

        return await PaginationHelper.CreateAsync(query,
          memberParams.PageNumber, memberParams.PageSize);
    }
}
