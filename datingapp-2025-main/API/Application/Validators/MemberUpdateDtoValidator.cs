using API.Application.DTOs;
using FluentValidation;

namespace API.Application.Validators;

public class MemberUpdateDtoValidator : AbstractValidator<MemberUpdateDto>
{
    public MemberUpdateDtoValidator()
    {
        RuleFor(x => x.DisplayName)
            .MaximumLength(50)
            .When(x => x.DisplayName != null);

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => x.Description != null);

        RuleFor(x => x.City)
            .MaximumLength(100)
            .When(x => x.City != null);

        RuleFor(x => x.Country)
            .MaximumLength(100)
            .When(x => x.Country != null);
    }
}
