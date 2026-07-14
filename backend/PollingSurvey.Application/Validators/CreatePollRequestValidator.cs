using FluentValidation;
using PollingSurvey.Application.DTOs;

namespace PollingSurvey.Application.Validators;

public class CreatePollRequestValidator : AbstractValidator<CreatePollRequest>
{
    public CreatePollRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.Questions)
            .NotEmpty().WithMessage("At least one question is required.");

        RuleForEach(x => x.Questions).SetValidator(new CreateQuestionRequestValidator());
    }
}

public class CreateQuestionRequestValidator : AbstractValidator<CreateQuestionRequest>
{
    private static readonly string[] ValidTypes =
        { "multiple_choice", "yes_no", "rating", "open_text" };

    public CreateQuestionRequestValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Question text is required.")
            .MaximumLength(500).WithMessage("Question text must not exceed 500 characters.");

        RuleFor(x => x.Type)
            .Must(type => ValidTypes.Contains(type))
            .WithMessage($"Question type must be one of: {string.Join(", ", ValidTypes)}.");

        RuleFor(x => x.Options)
            .Must((question, options) =>
                (question.Type != "multiple_choice" && question.Type != "yes_no")
                || options.Count >= 2)
            .WithMessage("multiple_choice and yes_no questions require at least 2 options.");
    }
}