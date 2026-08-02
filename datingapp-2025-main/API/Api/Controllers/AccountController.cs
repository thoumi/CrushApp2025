using System;
using System.Security.Cryptography;
using System.Text;
using API.Api.Extensions;
using API.Application.DTOs;
using API.Application.Interfaces;
using API.Domain.Entities;
using API.Domain.ValueObjects;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Api.Controllers;

public class AccountController(UserManager<AppUser> userManager, ITokenService tokenService,
    IValidator<RegisterDto> registerValidator) : BaseApiController
{
    [HttpGet("check-users")] // api/account/check-users - Endpoint de diagnostic
    public ActionResult CheckUsers()
    {
        return Ok(new { message = "API is working" });
    }

    [HttpPost("register")] // api/account/register
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        var validationResult = await registerValidator.ValidateAsync(registerDto);
        if (!validationResult.IsValid)
        {
            validationResult.AddToModelState(ModelState);
            return ValidationProblem();
        }

        // Garde-fou domaine, en plus de la validation FluentValidation ci-dessus :
        // Email reste valide même si ce contrôleur est un jour contourné.
        Email email;
        try
        {
            email = Email.Create(registerDto.Email);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

        var user = new AppUser
        {
            DisplayName = registerDto.DisplayName,
            Email = email,
            UserName = email,
            Member = new Member
            {
                DisplayName = registerDto.DisplayName,
                Gender = registerDto.Gender,
                City = registerDto.City,
                Country = registerDto.Country,
                DateOfBirth = registerDto.DateOfBirth
            }
        };

        var result = await userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("identity", error.Description);
            }

            return ValidationProblem();
        }

        await userManager.AddToRoleAsync(user, "Member");

        await SetRefreshTokenCookie(user);

        return await user.ToDto(tokenService);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await userManager.FindByEmailAsync(loginDto.Email);

        if (user == null) return Unauthorized("Invalid email address");

        var result = await userManager.CheckPasswordAsync(user, loginDto.Password);

        if (!result) return Unauthorized("Invalid password");

        await SetRefreshTokenCookie(user);

        return await user.ToDto(tokenService);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<UserDto>> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (refreshToken == null) return NoContent();

        var user = await userManager.Users
            .FirstOrDefaultAsync(x => x.RefreshToken == refreshToken
                && x.RefreshTokenExpiry > DateTime.UtcNow);

        if (user == null) return Unauthorized();

        await SetRefreshTokenCookie(user);

        return await user.ToDto(tokenService);
    }

    private async Task SetRefreshTokenCookie(AppUser user)
    {
        var refreshToken = tokenService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await userManager.UpdateAsync(user);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // Changed to false for development (localhost HTTP)
            SameSite = SameSiteMode.Lax, // Changed to Lax for development
            Expires = DateTime.UtcNow.AddDays(7)
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        await userManager.Users
            .Where(x => x.Id == User.GetMemberId())
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.RefreshToken, _ => null)
                .SetProperty(x => x.RefreshTokenExpiry, _ => null)
                );

        Response.Cookies.Delete("refreshToken");

        return Ok();
    }
}
