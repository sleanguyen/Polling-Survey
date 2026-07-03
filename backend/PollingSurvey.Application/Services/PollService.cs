using PollingSurvey.Application.Common;
using PollingSurvey.Application.DTOs;
using PollingSurvey.Application.Interfaces;
using PollingSurvey.Application.Repositories;
using PollingSurvey.Domain.Entities;

namespace PollingSurvey.Application.Services;

public class PollService : IPollService
{
    private readonly IPollRepository _repository;
    private readonly IPollNotifier _notifier;

    public PollService(IPollRepository repository, IPollNotifier notifier)
    {
        _repository = repository;
        _notifier = notifier;
    }

    public async Task<ServiceResult<PollResponse>> CreatePollAsync(CreatePollRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return ServiceResult<PollResponse>.ValidationError("Title is required.");

        if (request.Questions == null || request.Questions.Count == 0)
            return ServiceResult<PollResponse>.ValidationError("At least one question is required.");

        var poll = new Poll
        {
            Code = GenerateShortCode(),
            Title = request.Title,
            ExpiresAt = request.ExpiresAt,
            Questions = request.Questions.Select(q => new Question
            {
                Text = q.Text,
                Type = q.Type,
                Order = q.Order,
                Options = q.Options.Select(o => new Option
                {
                    Text = o.Text,
                    Order = o.Order
                }).ToList()
            }).ToList()
        };

        await _repository.AddPollAsync(poll);
        await _repository.SaveChangesAsync();

        return ServiceResult<PollResponse>.Success(MapToPollResponse(poll));
    }

    public async Task<ServiceResult<PollResponse>> GetPollAsync(string code)
    {
        var poll = await _repository.GetPollByCodeAsync(code);

        if (poll == null)
            return ServiceResult<PollResponse>.NotFound("Poll not found.");

        if (poll.ExpiresAt.HasValue && poll.ExpiresAt < DateTime.UtcNow && poll.Status == "open")
        {
            poll.Status = "closed";
            await _repository.SaveChangesAsync();
        }

        return ServiceResult<PollResponse>.Success(MapToPollResponse(poll));
    }

    public async Task<ServiceResult<string>> SubmitVoteAsync(string code, SubmitVoteRequest request)
    {
        var poll = await _repository.GetPollByCodeAsync(code);

        if (poll == null)
            return ServiceResult<string>.NotFound("Poll not found.");

        if (poll.Status == "closed")
            return ServiceResult<string>.Forbidden("This poll is closed.");

        if (poll.ExpiresAt.HasValue && poll.ExpiresAt < DateTime.UtcNow)
            return ServiceResult<string>.Forbidden("This poll has expired.");

        var alreadyVoted = await _repository.HasUserVotedAsync(request.QuestionId, request.VoterToken);
        if (alreadyVoted)
            return ServiceResult<string>.Conflict("You have already voted on this question.");

        await _repository.AddVoteAsync(new Vote
        {
            QuestionId = request.QuestionId,
            OptionId = request.OptionId,
            RatingValue = request.RatingValue,
            OpenTextValue = request.OpenTextValue,
            VoterToken = request.VoterToken
        });
        await _repository.SaveChangesAsync();

        // ✅ Tính lại kết quả và broadcast cho group của poll này
        var updatedResults = await BuildResultsAsync(code);
        if (updatedResults != null)
            await _notifier.BroadcastPollUpdateAsync(code, updatedResults);

        return ServiceResult<string>.Success("Vote submitted successfully.");
    }

    public async Task<ServiceResult<PollResultResponse>> GetResultsAsync(string code)
    {
        var result = await BuildResultsAsync(code);

        if (result == null)
            return ServiceResult<PollResultResponse>.NotFound("Poll not found.");

        return ServiceResult<PollResultResponse>.Success(result);
    }

    public async Task<ServiceResult<string>> ClosePollAsync(string code)
    {
        var poll = await _repository.GetPollBasicByCodeAsync(code);

        if (poll == null)
            return ServiceResult<string>.NotFound("Poll not found.");

        if (poll.Status == "closed")
            return ServiceResult<string>.ValidationError("Poll is already closed.");

        poll.Status = "closed";
        await _repository.SaveChangesAsync();

        return ServiceResult<string>.Success("Poll closed successfully.");
    }

    // ✅ Tách ra dùng chung cho GetResults và SubmitVote
    private async Task<PollResultResponse?> BuildResultsAsync(string code)
    {
        var poll = await _repository.GetPollWithVotesByCodeAsync(code);

        if (poll == null) return null;

        return new PollResultResponse
        {
            PollId = poll.Id,
            Code = poll.Code,
            Title = poll.Title,
            Status = poll.Status,
            Questions = poll.Questions.OrderBy(q => q.Order).Select(q =>
            {
                var totalVotes = q.Votes.Count;

                var questionResult = new QuestionResultResponse
                {
                    QuestionId = q.Id,
                    Text = q.Text,
                    Type = q.Type,
                    TotalVotes = totalVotes
                };

                if (q.Type == "multiple_choice" || q.Type == "yes_no")
                {
                    questionResult.Options = q.Options.OrderBy(o => o.Order).Select(o =>
                    {
                        var count = q.Votes.Count(v => v.OptionId == o.Id);
                        return new OptionResultResponse
                        {
                            OptionId = o.Id,
                            Text = o.Text,
                            VoteCount = count,
                            Percentage = totalVotes > 0
                                ? Math.Round((double)count / totalVotes * 100, 1)
                                : 0
                        };
                    }).ToList();
                }
                else if (q.Type == "rating")
                {
                    var ratings = q.Votes
                        .Where(v => v.RatingValue.HasValue)
                        .Select(v => v.RatingValue!.Value)
                        .ToList();
                    questionResult.AverageRating = ratings.Count > 0
                        ? Math.Round(ratings.Average(), 2)
                        : null;
                }
                else if (q.Type == "open_text")
                {
                    questionResult.OpenTextAnswers = q.Votes
                        .Where(v => !string.IsNullOrEmpty(v.OpenTextValue))
                        .Select(v => v.OpenTextValue!)
                        .ToList();
                }

                return questionResult;
            }).ToList()
        };
    }

    private static string GenerateShortCode(int length = 6)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private static PollResponse MapToPollResponse(Poll poll) => new()
    {
        Id = poll.Id,
        Code = poll.Code,
        Title = poll.Title,
        Status = poll.Status,
        CreatedAt = poll.CreatedAt,
        ExpiresAt = poll.ExpiresAt,
        Questions = poll.Questions.OrderBy(q => q.Order).Select(q => new QuestionResponse
        {
            Id = q.Id,
            Text = q.Text,
            Type = q.Type,
            Order = q.Order,
            Options = q.Options.OrderBy(o => o.Order).Select(o => new OptionResponse
            {
                Id = o.Id,
                Text = o.Text,
                Order = o.Order
            }).ToList()
        }).ToList()
    };
}