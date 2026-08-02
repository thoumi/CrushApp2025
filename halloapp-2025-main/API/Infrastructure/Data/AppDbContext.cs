using System;
using API.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace API.Infrastructure.Data;

public class AppDbContext(DbContextOptions options) : IdentityDbContext<AppUser>(options)
{
    public DbSet<Member> Members { get; set; }
    public DbSet<Photo> Photos { get; set; }
    public DbSet<MemberLike> Likes { get; set; }
    public DbSet<Match> Matches { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Connection> Connections { get; set; }
    public DbSet<Prompt> Prompts { get; set; }
    public DbSet<PromptAnswer> PromptAnswers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.ConfigureWarnings(warnings => 
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Photo>().HasQueryFilter(x => x.IsApproved);

        modelBuilder.Entity<IdentityRole>()
            .HasData(
                new IdentityRole { Id = "member-id", Name = "Member", NormalizedName = "MEMBER" },
                new IdentityRole { Id = "moderator-id", Name = "Moderator", NormalizedName = "MODERATOR" },
                new IdentityRole { Id = "admin-id", Name = "Admin", NormalizedName = "ADMIN" }
            );

        modelBuilder.Entity<Message>()
            .HasOne(x => x.Recipient)
            .WithMany(m => m.MessagesReceived)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(x => x.Sender)
            .WithMany(m => m.MessagesSent)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MemberLike>()
            .HasKey(x => new { x.SourceMemberId, x.TargetMemberId });

        modelBuilder.Entity<MemberLike>()
            .HasOne(s => s.SourceMember)
            .WithMany(t => t.LikedMembers)
            .HasForeignKey(s => s.SourceMemberId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MemberLike>()
            .HasOne(s => s.TargetMember)
            .WithMany(t => t.LikedByMembers)
            .HasForeignKey(s => s.TargetMemberId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Match>()
            .HasOne(m => m.MemberOne)
            .WithMany()
            .HasForeignKey(m => m.MemberOneId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Match>()
            .HasOne(m => m.MemberTwo)
            .WithMany()
            .HasForeignKey(m => m.MemberTwoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Match>()
            .HasIndex(m => new { m.MemberOneId, m.MemberTwoId })
            .IsUnique();

        modelBuilder.Entity<PromptAnswer>()
            .HasOne(x => x.Member)
            .WithMany(m => m.PromptAnswers)
            .HasForeignKey(x => x.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PromptAnswer>()
            .HasOne(x => x.Prompt)
            .WithMany()
            .HasForeignKey(x => x.PromptId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PromptAnswer>()
            .HasIndex(x => new { x.MemberId, x.PromptId })
            .IsUnique();

        modelBuilder.Entity<Prompt>().HasData(
            new Prompt { Id = 1, Text = "Un talent inutile mais fascinant que je possède…" },
            new Prompt { Id = 2, Text = "Mon dimanche parfait ressemble à…" },
            new Prompt { Id = 3, Text = "Je suis convaincu·e que…" },
            new Prompt { Id = 4, Text = "La dernière chose qui m'a fait rire aux éclats…" },
            new Prompt { Id = 5, Text = "Je cherche quelqu'un qui…" },
            new Prompt { Id = 6, Text = "Mon pire talent culinaire…" },
            new Prompt { Id = 7, Text = "Un sujet sur lequel je peux parler pendant des heures…" },
            new Prompt { Id = 8, Text = "La prochaine destination sur ma liste…" },
            new Prompt { Id = 9, Text = "Ma bande-son du moment…" },
            new Prompt { Id = 10, Text = "Une habitude bizarre que j'assume totalement…" },
            new Prompt { Id = 11, Text = "Le meilleur conseil qu'on m'ait donné…" },
            new Prompt { Id = 12, Text = "Je suis plutôt du genre à…" },
            new Prompt { Id = 13, Text = "Ce qui compte le plus pour moi dans une relation…" },
            new Prompt { Id = 14, Text = "Un fait à mon sujet qui surprend toujours…" },
            new Prompt { Id = 15, Text = "Le film ou le livre que je peux revoir sans jamais me lasser…" },
            new Prompt { Id = 16, Text = "La question que j'aimerais qu'on me pose plus souvent…" }
        );

        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
            v => v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
        );

        var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue ? v.Value.ToUniversalTime() : null,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null
        );

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(dateTimeConverter);
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(nullableDateTimeConverter);
                }
            }
        }
    }
}
