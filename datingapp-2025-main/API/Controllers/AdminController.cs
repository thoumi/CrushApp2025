using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace API.Controllers;

public class AdminController(UserManager<AppUser> userManager, IUnitOfWork uow,
    IPhotoService photoService) : BaseApiController
{
    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult<PaginatedResult<UsersManageDto>>> GetUsersWithRoles([FromQuery] MemberParams memberParams)
    {
        return Ok(await uow.AdminRepository.GetUsersWithRoles(memberParams));       
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("edit-roles/{userId}")]
    public async Task<ActionResult<IList<string>>> EditRoles(string userId, [FromQuery] string roles)
    {
        if (string.IsNullOrEmpty(roles)) return BadRequest("You must select at least one role");

        var selectedRoles = roles.Split(",").ToArray();

        var user = await userManager.FindByIdAsync(userId);

        if (user == null) return BadRequest("Could not retrieve user");

        var userRoles = await userManager.GetRolesAsync(user);

        var result = await userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

        if (!result.Succeeded) return BadRequest("Failed to add to roles");

        result = await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

        if (!result.Succeeded) return BadRequest("Failed to remove from roles");

        return Ok(await userManager.GetRolesAsync(user));

    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-to-moderate")]
    public async Task<ActionResult<IEnumerable<Photo>>> GetPhotosForModeration()
    {
        return Ok(await uow.PhotoRepository.GetUnapprovedPhotos());
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("approve-photo/{photoId}")]
    public async Task<ActionResult> ApprovePhoto(int photoId)
    {
        var photo = await uow.PhotoRepository.GetPhotoById(photoId);

        if (photo == null) return BadRequest("Could not get photo from db");

        var member = await uow.MemberRepository.GetMemberForUpdate(photo.MemberId);

        if (member == null) return BadRequest("Could not get member");

        photo.IsApproved = true;

        if (member.ImageUrl == null)
        {
            member.ImageUrl = photo.Url;
            member.User.ImageUrl = photo.Url;
        }

        await uow.Complete();

        return Ok();
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("reject-photo/{photoId}")]
    public async Task<ActionResult> RejectPhoto(int photoId)
    {
        var photo = await uow.PhotoRepository.GetPhotoById(photoId);

        if (photo == null) return BadRequest("Could not get photo from db");

        if (photo.PublicId != null)
        {
            var result = await photoService.DeletePhotoAsync(photo.PublicId);

            if (result.Result == "ok")
            {
                uow.PhotoRepository.RemovePhoto(photo);
            }
        }
        else
        {
            uow.PhotoRepository.RemovePhoto(photo);
        }

        await uow.Complete();

        return Ok();
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("toggle-user-status/{userId}")]
    public async Task<ActionResult> BlockUser(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return NotFound("Could not find user");

        bool isLocked = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.Now;

        IdentityResult result;

        if (isLocked)
        {
            // D�bloquer
            result = await userManager.SetLockoutEndDateAsync(user, null);
            if (!result.Succeeded) return BadRequest("Failed to unlock user");
            return Ok(new { Message = $"User {user.UserName} has been unlocked successfully.", isLockedOut = false });
        }
        else
        {
            // Bloquer
            result = await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            if (!result.Succeeded) return BadRequest("Failed to lock user");
            return Ok(new { Message = $"User {user.UserName} has been blocked successfully.", isLockedOut = true });
        }
    }
}
