using API.Application.DTOs;
using FluentValidation;

namespace API.Application.Validators;

public class SavePromptAnswersDtoValidator : AbstractValidator<SavePromptAnswersDto>
{
    public SavePromptAnswersDtoValidator()
    {
        RuleFor(x => x.Answers)
            .NotEmpty().WithMessage("Choisissez entre 1 et 3 prompts.")
            .Must(answers => answers.Count <= 3).WithMessage("Choisissez entre 1 et 3 prompts.");

        RuleForEach(x => x.Answers).ChildRules(answer =>
        {
            answer.RuleFor(a => a.Answer)
                .NotEmpty().WithMessage("La réponse ne peut pas être vide.")
                .MaximumLength(300);
        });
    }
}
