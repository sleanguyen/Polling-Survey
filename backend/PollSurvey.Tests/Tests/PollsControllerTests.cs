using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using PollingSurvey.API.Controllers;
using PollingSurvey.API.Hubs;
using PollSurvey.API.Data;
using PollSurvey.API.DTOs;

namespace PollSurvey.Tests;

public class PollsControllerTests
{
    // Tạo InMemory database mới cho mỗi test — tránh data bị ảnh hưởng lẫn nhau
    private AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    // Tạo PollsController với IHubContext<PollHub> giả (mock) — controller cần
    // tham số này từ khi thêm SignalR broadcast, nhưng test không cần SignalR thật
    private static PollsController CreateController(AppDbContext db)
    {
        var mockHub = new Mock<IHubContext<PollHub>>();
        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();

        mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);
        mockHub.Setup(h => h.Clients).Returns(mockClients.Object);

        return new PollsController(db, mockHub.Object);
    }

    // ✅ Test 1: Tạo poll hợp lệ → trả về 201 Created
    [Fact]
    public async Task CreatePoll_ValidRequest_Returns201()
    {
        var db = CreateDb();
        var controller = CreateController(db);

        var request = new CreatePollRequest
        {
            Title = "Favourite color?",
            Questions = new()
            {
                new CreateQuestionRequest
                {
                    Text = "What is your favourite color?",
                    Type = "multiple_choice",
                    Order = 1,
                    Options = new()
                    {
                        new CreateOptionRequest { Text = "Red", Order = 1 },
                        new CreateOptionRequest { Text = "Blue", Order = 2 }
                    }
                }
            }
        };

        var result = await controller.CreatePoll(request);

        result.Should().BeOfType<CreatedAtActionResult>()
            .Which.StatusCode.Should().Be(201);
    }

    // ✅ Test 2: Tạo poll không có title → trả về 400
    [Fact]
    public async Task CreatePoll_EmptyTitle_Returns400()
    {
        var db = CreateDb();
        var controller = CreateController(db);

        var request = new CreatePollRequest
        {
            Title = "",
            Questions = new()
            {
                new CreateQuestionRequest { Text = "Q1", Type = "multiple_choice", Order = 1 }
            }
        };

        var result = await controller.CreatePoll(request);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.StatusCode.Should().Be(400);
    }

    // ✅ Test 3: Tạo poll không có question → trả về 400
    [Fact]
    public async Task CreatePoll_NoQuestions_Returns400()
    {
        var db = CreateDb();
        var controller = CreateController(db);

        var request = new CreatePollRequest
        {
            Title = "Test Poll",
            Questions = new()
        };

        var result = await controller.CreatePoll(request);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.StatusCode.Should().Be(400);
    }

    // ✅ Test 4: Lấy poll không tồn tại → trả về 404
    [Fact]
    public async Task GetPoll_NotFound_Returns404()
    {
        var db = CreateDb();
        var controller = CreateController(db);

        var result = await controller.GetPoll("wrongcode");

        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.StatusCode.Should().Be(404);
    }

    // ✅ Test 5: Vote trùng cùng voter token → trả về 409 Conflict
    [Fact]
    public async Task SubmitVote_DuplicateVote_Returns409()
    {
        var db = CreateDb();
        var controller = CreateController(db);

        // Tạo poll trước
        var createRequest = new CreatePollRequest
        {
            Title = "Test Poll",
            Questions = new()
            {
                new CreateQuestionRequest
                {
                    Text = "Pick one",
                    Type = "multiple_choice",
                    Order = 1,
                    Options = new()
                    {
                        new CreateOptionRequest { Text = "Yes", Order = 1 }
                    }
                }
            }
        };

        var createResult = await controller.CreatePoll(createRequest) as CreatedAtActionResult;
        var poll = createResult!.Value as PollSurvey.API.DTOs.PollResponse;
        var questionId = poll!.Questions[0].Id;
        var optionId = poll.Questions[0].Options[0].Id;

        var voteRequest = new SubmitVoteRequest
        {
            QuestionId = questionId,
            OptionId = optionId,
            VoterToken = "token-abc-123"
        };

        // Vote lần 1 — thành công
        await controller.SubmitVote(poll.Code, voteRequest);

        // Vote lần 2 cùng token — phải bị conflict
        var result = await controller.SubmitVote(poll.Code, voteRequest);

        result.Should().BeOfType<ConflictObjectResult>()
            .Which.StatusCode.Should().Be(409);
    }

    // ✅ Test 6: Đóng poll → status chuyển thành closed
    [Fact]
    public async Task ClosePoll_OpenPoll_ReturnsOk()
    {
        var db = CreateDb();
        var controller = CreateController(db);

        var createRequest = new CreatePollRequest
        {
            Title = "Poll to close",
            Questions = new()
            {
                new CreateQuestionRequest
                {
                    Text = "Q?", Type = "multiple_choice", Order = 1,
                    Options = new() { new CreateOptionRequest { Text = "A", Order = 1 } }
                }
            }
        };

        var createResult = await controller.CreatePoll(createRequest) as CreatedAtActionResult;
        var poll = createResult!.Value as PollSurvey.API.DTOs.PollResponse;

        var result = await controller.ClosePoll(poll!.Code);

        result.Should().BeOfType<OkObjectResult>()
            .Which.StatusCode.Should().Be(200);
    }

    // ✅ Test 7: Vote vào poll đã đóng → trả về 403
    [Fact]
    public async Task SubmitVote_ClosedPoll_Returns403()
    {
        var db = CreateDb();
        var controller = CreateController(db);

        var createRequest = new CreatePollRequest
        {
            Title = "Closed Poll",
            Questions = new()
            {
                new CreateQuestionRequest
                {
                    Text = "Q?", Type = "multiple_choice", Order = 1,
                    Options = new() { new CreateOptionRequest { Text = "A", Order = 1 } }
                }
            }
        };

        var createResult = await controller.CreatePoll(createRequest) as CreatedAtActionResult;
        var poll = createResult!.Value as PollSurvey.API.DTOs.PollResponse;

        // Đóng poll trước
        await controller.ClosePoll(poll!.Code);

        var voteRequest = new SubmitVoteRequest
        {
            QuestionId = poll.Questions[0].Id,
            OptionId = poll.Questions[0].Options[0].Id,
            VoterToken = "token-xyz"
        };

        var result = await controller.SubmitVote(poll.Code, voteRequest);

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(403);
    }
}