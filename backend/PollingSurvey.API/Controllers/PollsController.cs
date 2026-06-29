using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PollSurvey.API.Data;
using PollSurvey.API.DTOs;
using PollingSurvey.API.Hubs;
using PollSurvey.API.Models;

namespace PollingSurvey.API.Controllers;

[ApiController]
[Route("api/polls")]
public class PollsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IHubContext<PollHub> _hub; // ✅ thêm

    public PollsController(AppDbContext context, IHubContext<PollHub> hub) // ✅ thêm hub
    {
        _context = context;
        _hub = hub;
    }

    // POST api/polls
    [HttpPost]
    public async Task<IActionResult> CreatePoll([FromBody] CreatePollRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { message = "Title is required." });

        if (request.Questions == null || request.Questions.Count == 0)
            return BadRequest(new { message = "At least one question is required." });

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

        _context.Polls.Add(poll);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPoll), new { code = poll.Code }, MapToPollResponse(poll));
    }

    // GET api/polls/{code}
    [HttpGet("{code}")]
    public async Task<IActionResult> GetPoll(string code)
    {
        var poll = await _context.Polls
            .Include(p => p.Questions)
                .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(p => p.Code == code);

        if (poll == null)
            return NotFound(new { message = "Poll not found." });

        if (poll.ExpiresAt.HasValue && poll.ExpiresAt < DateTime.UtcNow && poll.Status == "open")
        {
            poll.Status = "closed";
            await _context.SaveChangesAsync();
        }

        return Ok(MapToPollResponse(poll));
    }

    // POST api/polls/{code}/vote ✅ thêm broadcast
    [HttpPost("{code}/vote")]
    public async Task<IActionResult> SubmitVote(string code, [FromBody] SubmitVoteRequest request)
    {
        var poll = await _context.Polls
            .Include(p => p.Questions)
                .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(p => p.Code == code);

        if (poll == null)
            return NotFound(new { message = "Poll not found." });

        if (poll.Status == "closed")
            return StatusCode(403, new { message = "This poll is closed." });

        if (poll.ExpiresAt.HasValue && poll.ExpiresAt < DateTime.UtcNow)
            return StatusCode(403, new { message = "This poll has expired." });

        var alreadyVoted = await _context.Votes
            .AnyAsync(v => v.QuestionId == request.QuestionId && v.VoterToken == request.VoterToken);

        if (alreadyVoted)
            return Conflict(new { message = "You have already voted on this question." });

        _context.Votes.Add(new Vote
        {
            QuestionId = request.QuestionId,
            OptionId = request.OptionId,
            RatingValue = request.RatingValue,
            OpenTextValue = request.OpenTextValue,
            VoterToken = request.VoterToken
        });
        await _context.SaveChangesAsync();

        // ✅ Tính lại kết quả và broadcast cho group của poll này
        var updatedResults = await BuildResultsAsync(code);
        await _hub.Clients.Group(code).SendAsync("ReceivePollUpdate", updatedResults);

        return Ok(new { message = "Vote submitted successfully." });
    }

    // GET api/polls/{code}/results
    [HttpGet("{code}/results")]
    public async Task<IActionResult> GetResults(string code)
    {
        var result = await BuildResultsAsync(code);
        if (result == null)
            return NotFound(new { message = "Poll not found." });

        return Ok(result);
    }

    // PATCH api/polls/{code}/close
    [HttpPatch("{code}/close")]
    public async Task<IActionResult> ClosePoll(string code)
    {
        var poll = await _context.Polls.FirstOrDefaultAsync(p => p.Code == code);

        if (poll == null)
            return NotFound(new { message = "Poll not found." });

        if (poll.Status == "closed")
            return BadRequest(new { message = "Poll is already closed." });

        poll.Status = "closed";
        await _context.SaveChangesAsync();

        return Ok(new { message = "Poll closed successfully." });
    }

    // ✅ Tách ra dùng chung cho GetResults và SubmitVote
    private async Task<PollResultResponse?> BuildResultsAsync(string code)
    {
        var poll = await _context.Polls
            .Include(p => p.Questions)
                .ThenInclude(q => q.Options)
            .Include(p => p.Questions)
                .ThenInclude(q => q.Votes)
            .FirstOrDefaultAsync(p => p.Code == code);

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