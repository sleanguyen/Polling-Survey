using PollingSurvey.Domain.Entities;

namespace PollingSurvey.Application.Repositories;

public interface IPollRepository
{
    Task AddPollAsync(Poll poll);

    // Questions + Options — dùng cho CreatePoll (response) và GetPoll
    Task<Poll?> GetPollByCodeAsync(string code);

    // Questions + Options + Votes — dùng để tính kết quả (SubmitVote, GetResults)
    Task<Poll?> GetPollWithVotesByCodeAsync(string code);

    // Không Include gì — dùng cho ClosePoll, tránh load dư thừa
    Task<Poll?> GetPollBasicByCodeAsync(string code);

    Task SaveChangesAsync();

    Task<bool> HasUserVotedAsync(Guid questionId, string voterToken);

    Task AddVoteAsync(Vote vote);
}