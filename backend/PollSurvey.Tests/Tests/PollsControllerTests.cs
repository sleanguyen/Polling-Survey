using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using PollingSurvey.API.Controllers;
using PollingSurvey.API.Hubs;
using PollSurvey.API.Data;
using PollSurvey.API.DTOs;
using System.Timers;
using Xunit;

namespace PollSurvey.Tests;

public class PollsControllerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IHubContext<PollHub>> _hubMock;
    private readonly Mock<IClientProxy> _clientProxyMock;
    private readonly PollsController _controller;

    public PollsControllerTests()
    {
        // In-memory database để test độc lập
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // mỗi test 1 DB riêng
            .Options;

        _context = new AppDbContext(options);

        // Mock IHubContext<PollHub>
        _clientProxyMock = new Mock<IClientProxy>();

        var hubClientsMock = new Mock<IHubClients>();
        hubClientsMock
            .Setup(c => c.Group(It.IsAny<string>()))
            .Returns(_clientProxyMock.Object);

        _hubMock = new Mock<IHubContext<PollHub>>();
        _hubMock
            .Setup(h => h.Clients)
            .Returns(hubClientsMock.Object);

        // ✅ Truyền đủ 2 tham số vào constructor
        _controller = new PollsController(_context, _hubMock.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    // ─── CREATE POLL ───────────────────────────────────────────────

    [Fact]
    public async Task CreatePoll_ValidRequest_ReturnsCreated()
    {
        var request = new CreatePollRequest
        {
            Title = "Test Poll",
            Questions = new List<CreateQuestionRequest>
            {
                new()
                {
                    Text = "Question 1",
                    Type = "multiple_choice",
                    Order = 1,
                    Options = new List<CreateOptionRequest>
                    {
                        new() { Text = "Option A", Order = 1 },
                        new() { Text = "Option B", Order = 2 }
                    }
                }
            }
        };

        var result = await _controller.CreatePoll(request);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var response = Assert.IsType<PollResponse>(created.Value);
        Assert.Equal("Test Poll", response.Title);
        Assert.Equal("open", response.Status);
    }

    [Fact]
    public async Task CreatePoll_EmptyTitle_ReturnsBadRequest()
    {
        var request = new CreatePollRequest
        {
            Title = "",
            Questions = new List<CreateQuestionRequest>
            {
                new() { Text = "Q1", Type = "multiple_choice", Order = 1, Options = new() }
            }
        };

        var result = await _controller.CreatePoll(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreatePoll_NoQuestions_ReturnsBadRequest()
    {
        var request = new CreatePollRequest
        {
            Title = "Poll Without Questions",
            Questions = new List<CreateQuestionRequest>()
        };

        var result = await _controller.CreatePoll(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ─── GET POLL ──────────────────────────────────────────────────

    [Fact]
    public async Task GetPoll_ExistingCode_ReturnsPoll()
    {
        // Arrange: tạo poll trước
        var createResult = await _controller.CreatePoll(MakeValidRequest("Poll A"));
        var created = ((CreatedAtActionResult)createResult).Value as PollResponse;

        // Act
        var result = await _controller.GetPoll(created!.Code);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<PollResponse>(ok.Value);
        Assert.Equal(created.Code, response.Code);
    }

    [Fact]
    public async Task GetPoll_NotFound_Returns404()
    {
        var result = await _controller.GetPoll("ZZZZZZ");

        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ─── SUBMIT VOTE ───────────────────────────────────────────────

    [Fact]
    public async Task SubmitVote_ValidVote_ReturnsOkAndBroadcasts()
    {
        // Arrange
        var createResult = await _controller.CreatePoll(MakeValidRequest("Vote Poll"));
        var poll = ((CreatedAtActionResult)createResult).Value as PollResponse;
        var question = poll!.Questions.First();
        var option = question.Options.First();

        var voteRequest = new SubmitVoteRequest
        {
            QuestionId = question.Id,
            OptionId = option.Id,
            VoterToken = "voter-001"
        };

        // Act
        var result = await _controller.SubmitVote(poll.Code, voteRequest);

        // Assert: response OK
        Assert.IsType<OkObjectResult>(result);

        // Assert: SignalR broadcast được gọi
        _clientProxyMock.Verify(
            c => c.SendCoreAsync(
                "ReceivePollUpdate",
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SubmitVote_DuplicateVote_ReturnsConflict()
    {
        var createResult = await _controller.CreatePoll(MakeValidRequest("Dup Poll"));
        var poll = ((CreatedAtActionResult)createResult).Value as PollResponse;
        var question = poll!.Questions.First();
        var option = question.Options.First();

        var voteRequest = new SubmitVoteRequest
        {
            QuestionId = question.Id,
            OptionId = option.Id,
            VoterToken = "voter-dup"
        };

        await _controller.SubmitVote(poll.Code, voteRequest);
        var result = await _controller.SubmitVote(poll.Code, voteRequest); // lần 2

        Assert.IsType<ConflictObjectResult>(result);
    }

    // ─── CLOSE POLL ────────────────────────────────────────────────

    [Fact]
    public async Task ClosePoll_OpenPoll_ReturnsOk()
    {
        var createResult = await _controller.CreatePoll(MakeValidRequest("Close Me"));
        var poll = ((CreatedAtActionResult)createResult).Value as PollResponse;

        var result = await _controller.ClosePoll(poll!.Code);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task ClosePoll_AlreadyClosed_ReturnsBadRequest()
    {
        var createResult = await _controller.CreatePoll(MakeValidRequest("Already Closed"));
        var poll = ((CreatedAtActionResult)createResult).Value as PollResponse;

        await _controller.ClosePoll(poll!.Code);
        var result = await _controller.ClosePoll(poll.Code); // đóng lần 2

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ─── GET RESULTS ───────────────────────────────────────────────

    [Fact]
    public async Task GetResults_AfterVote_ReturnsCorrectPercentage()
    {
        var createResult = await _controller.CreatePoll(MakeValidRequest("Result Poll"));
        var poll = ((CreatedAtActionResult)createResult).Value as PollResponse;
        var question = poll!.Questions.First();
        var option = question.Options.First();

        await _controller.SubmitVote(poll.Code, new SubmitVoteRequest
        {
            QuestionId = question.Id,
            OptionId = option.Id,
            VoterToken = "voter-r1"
        });

        var result = await _controller.GetResults(poll.Code);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<PollResultResponse>(ok.Value);
        Assert.Equal(1, response.Questions.First().TotalVotes);
    }

    // ─── HELPER ────────────────────────────────────────────────────

    private static CreatePollRequest MakeValidRequest(string title) => new()
    {
        Title = title,
        Questions = new List<CreateQuestionRequest>
        {
            new()
            {
                Text = "Sample question?",
                Type = "multiple_choice",
                Order = 1,
                Options = new List<CreateOptionRequest>
                {
                    new() { Text = "Yes", Order = 1 },
                    new() { Text = "No", Order = 2 }
                }
            }
        }
    };
}