using Microsoft.EntityFrameworkCore;
using PollingSurvey.Application.Repositories;
using PollingSurvey.Domain.Entities;
using PollingSurvey.Infrastructure.Data;

namespace PollingSurvey.Infrastructure.Repositories;

public class PollRepository : IPollRepository
{
    private readonly AppDbContext _context;

    public PollRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddPollAsync(Poll poll)
    {
        await _context.Polls.AddAsync(poll);
    }

    public async Task<Poll?> GetPollByCodeAsync(string code)
    {
        return await _context.Polls
            .Include(p => p.Questions)
                .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(p => p.Code == code);
    }

    public async Task<Poll?> GetPollWithVotesByCodeAsync(string code)
    {
        return await _context.Polls
            .Include(p => p.Questions)
                .ThenInclude(q => q.Options)
            .Include(p => p.Questions)
                .ThenInclude(q => q.Votes)
            .FirstOrDefaultAsync(p => p.Code == code);
    }

    public async Task<Poll?> GetPollBasicByCodeAsync(string code)
    {
        return await _context.Polls.FirstOrDefaultAsync(p => p.Code == code);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasUserVotedAsync(Guid questionId, string voterToken)
    {
        return await _context.Votes.AnyAsync(v =>
            v.QuestionId == questionId &&
            v.VoterToken == voterToken);
    }

    public async Task AddVoteAsync(Vote vote)
    {
        await _context.Votes.AddAsync(vote);
    }
}