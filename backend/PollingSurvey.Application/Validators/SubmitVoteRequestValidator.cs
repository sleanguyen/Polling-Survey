using FluentValidation;
using PollingSurvey.Application.DTOs;

namespace PollingSurvey.Application.Validators;

public class SubmitVoteRequestValidator : AbstractValidator<SubmitVoteRequest>
{
    public SubmitVoteRequestValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("QuestionId is required.");

        RuleFor(x => x.VoterToken)
            .NotEmpty().WithMessage("VoterToken is required.");

        RuleFor(x => x)
            .Must(x => x.OptionId.HasValue || x.RatingValue.HasValue || !string.IsNullOrWhiteSpace(x.OpenTextValue))
            .WithMessage("A vote must include an OptionId, a RatingValue, or an OpenTextValue.");
    }
}