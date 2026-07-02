using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PollSurvey.API.DTOs;

namespace PollSurvey.Tests.IntegrationTests;

public class PollsApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PollsApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static CreatePollRequest BuildSamplePollRequest(string title) => new()
    {
        Title = title,
        Questions = new()
        {
            new CreateQuestionRequest
            {
                Text = "Which language do you use the most?",
                Type = "multiple_choice",
                Order = 1,
                Options = new()
                {
                    new CreateOptionRequest { Text = "C#", Order = 1 },
                    new CreateOptionRequest { Text = "Python", Order = 2 }
                }
            }
        }
    };

    // ✅ Integration Test 1: POST /api/polls → 201 Created qua HTTP thật
    [Fact]
    public async Task POST_CreatePoll_ValidRequest_Returns201()
    {
        var request = BuildSamplePollRequest("Integration Test Poll 1");

        var response = await _client.PostAsJsonAsync("/api/polls", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var poll = await response.Content.ReadFromJsonAsync<PollResponse>();
        poll.Should().NotBeNull();
        poll!.Code.Should().NotBeNullOrEmpty();
        poll.Questions.Should().HaveCount(1);
    }

    // ✅ Integration Test 2: GET /api/polls/{code} → 200, trả về đúng poll vừa tạo
    [Fact]
    public async Task GET_Poll_ExistingCode_Returns200WithCorrectData()
    {
        var createRequest = BuildSamplePollRequest("Integration Test Poll 2");
        var createResponse = await _client.PostAsJsonAsync("/api/polls", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<PollResponse>();

        var response = await _client.GetAsync($"/api/polls/{created!.Code}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var poll = await response.Content.ReadFromJsonAsync<PollResponse>();
        poll!.Title.Should().Be("Integration Test Poll 2");
        poll.Status.Should().Be("open");
    }

    // ✅ Integration Test 3: POST /api/polls/{code}/vote → 200, vote thành công
    [Fact]
    public async Task POST_SubmitVote_ValidVote_Returns200()
    {
        var createRequest = BuildSamplePollRequest("Integration Test Poll 3");
        var createResponse = await _client.PostAsJsonAsync("/api/polls", createRequest);
        var poll = await createResponse.Content.ReadFromJsonAsync<PollResponse>();

        var voteRequest = new SubmitVoteRequest
        {
            QuestionId = poll!.Questions[0].Id,
            OptionId = poll.Questions[0].Options[0].Id,
            VoterToken = "integration-token-1"
        };

        var response = await _client.PostAsJsonAsync($"/api/polls/{poll.Code}/vote", voteRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ✅ Integration Test 4: Vote trùng voter token → 409 Conflict
    [Fact]
    public async Task POST_SubmitVote_DuplicateToken_Returns409()
    {
        var createRequest = BuildSamplePollRequest("Integration Test Poll 4");
        var createResponse = await _client.PostAsJsonAsync("/api/polls", createRequest);
        var poll = await createResponse.Content.ReadFromJsonAsync<PollResponse>();

        var voteRequest = new SubmitVoteRequest
        {
            QuestionId = poll!.Questions[0].Id,
            OptionId = poll.Questions[0].Options[0].Id,
            VoterToken = "integration-token-2"
        };

        await _client.PostAsJsonAsync($"/api/polls/{poll.Code}/vote", voteRequest);
        var secondResponse = await _client.PostAsJsonAsync($"/api/polls/{poll.Code}/vote", voteRequest);

        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // ✅ Integration Test 5: GET /api/polls/{code}/results → 200, vote count đúng
    [Fact]
    public async Task GET_Results_AfterVoting_ReturnsCorrectVoteCount()
    {
        var createRequest = BuildSamplePollRequest("Integration Test Poll 5");
        var createResponse = await _client.PostAsJsonAsync("/api/polls", createRequest);
        var poll = await createResponse.Content.ReadFromJsonAsync<PollResponse>();

        var voteRequest = new SubmitVoteRequest
        {
            QuestionId = poll!.Questions[0].Id,
            OptionId = poll.Questions[0].Options[0].Id,
            VoterToken = "integration-token-3"
        };
        await _client.PostAsJsonAsync($"/api/polls/{poll.Code}/vote", voteRequest);

        var response = await _client.GetAsync($"/api/polls/{poll.Code}/results");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var results = await response.Content.ReadFromJsonAsync<PollResultResponse>();
        results!.Questions[0].TotalVotes.Should().Be(1);
        results.Questions[0].Options![0].VoteCount.Should().Be(1);
    }
}