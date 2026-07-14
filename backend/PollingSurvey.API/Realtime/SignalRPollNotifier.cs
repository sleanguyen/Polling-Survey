using Microsoft.AspNetCore.SignalR;
using PollingSurvey.API.Hubs;
using PollingSurvey.Application.DTOs;
using PollingSurvey.Application.Interfaces;

namespace PollingSurvey.API.Realtime;

public class SignalRPollNotifier : IPollNotifier
{
    private readonly IHubContext<PollHub> _hub;

    public SignalRPollNotifier(IHubContext<PollHub> hub)
    {
        _hub = hub;
    }

    public async Task BroadcastPollUpdateAsync(string pollCode, PollResultResponse result)
    {
        await _hub.Clients.Group(pollCode).SendAsync("ReceivePollUpdate", result);
    }
}