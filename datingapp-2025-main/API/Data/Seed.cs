using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class Seed
{
    public static async Task SeedUsers(UserManager<AppUser> userManager)
    {
        var userCount = await userManager.Users.CountAsync();
        Console.WriteLine($"📊 Current user count: {userCount}");
        
        // Always run seeding to ensure all users are created
        Console.WriteLine("🔄 Starting database seed (forced)...");

        Console.WriteLine("📦 Starting database seed...");
        
        var memberData = await File.ReadAllTextAsync("Data/UserSeedData.json");
        var members = JsonSerializer.Deserialize<List<SeedUserDto>>(memberData);

        if (members == null)
        {
            Console.WriteLine("❌ No members in seed data");
            return;
        }
        
        Console.WriteLine($"📋 Found {members.Count} users to seed");

        foreach (var member in members)
        {
            // Vérifier si l'utilisateur existe déjà
            var existingUser = await userManager.FindByIdAsync(member.Id);
            if (existingUser != null)
            {
                Console.WriteLine($"⏭️ User {member.Email} already exists - skipping");
                continue;
            }

            var user = new AppUser
            {
                Id = member.Id,
                Email = member.Email,
                UserName = member.Email,
                DisplayName = member.DisplayName,
                ImageUrl = member.ImageUrl,
                Member = new Member
                {
                    Id = member.Id,
                    DisplayName = member.DisplayName,
                    Description = member.Description,
                    DateOfBirth = member.DateOfBirth,
                    ImageUrl = member.ImageUrl,
                    Gender = member.Gender,
                    City = member.City,
                    Country = member.Country,
                    LastActive = member.LastActive,
                    Created = member.Created
                }
            };

            user.Member.Photos.Add(new Photo
            {
                Url = member.ImageUrl!,
                MemberId = member.Id,
                IsApproved = true
            });

            var result = await userManager.CreateAsync(user, "Pa$$w0rd");
            if (!result.Succeeded)
            {
                Console.WriteLine($"❌ Failed to create user {member.Email}: {result.Errors.First().Description}");
            }
            else
            {
                await userManager.AddToRoleAsync(user, "Member");
                Console.WriteLine($"✅ Created user: {member.Email}");
            }
        }

        Console.WriteLine($"✅ Successfully seeded {members.Count} members");
        
        var admin = new AppUser
        {
            UserName = "admin@test.com",
            Email = "admin@test.com",
            DisplayName = "Admin"
        };

        var adminResult = await userManager.CreateAsync(admin, "Pa$$w0rd");
        if (adminResult.Succeeded)
        {
            await userManager.AddToRolesAsync(admin, ["Admin", "Moderator"]);
            Console.WriteLine("✅ Admin user created successfully");
        }
        else
        {
            Console.WriteLine($"❌ Failed to create admin: {adminResult.Errors.First().Description}");
        }
        
        var totalUsers = await userManager.Users.CountAsync();
        Console.WriteLine($"🎉 Database seed completed! Total users: {totalUsers}");
    }
}
