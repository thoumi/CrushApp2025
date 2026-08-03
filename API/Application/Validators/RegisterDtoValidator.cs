using API.Application.DTOs;
using API.Domain.ValueObjects;
using FluentValidation;

namespace API.Application.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Le nom affiché est requis.")
            .MaximumLength(50);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("L'email est requis.")
            .EmailAddress().WithMessage("L'email n'est pas valide.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Le mot de passe est requis.")
            .MinimumLength(4).WithMessage("Le mot de passe doit contenir au moins 4 caractères.");

        RuleFor(x => x.Gender).NotEmpty();
        RuleFor(x => x.City).NotEmpty();
        RuleFor(x => x.Country).NotEmpty();

        RuleFor(x => x.DateOfBirth)
            .Must(BeAtLeastMinimumAge)
            .WithMessage($"Vous devez avoir au moins {AgeRange.MinimumAllowedAge} ans pour vous inscrire.");
    }

    private static bool BeAtLeastMinimumAge(DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(-age)) age--;
        return age >= AgeRange.MinimumAllowedAge;
    }
}
