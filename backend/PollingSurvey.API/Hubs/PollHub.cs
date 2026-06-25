using Microsoft.AspNetCore.SignalR;

namespace PollingSurvey.API.Hubs;

public class PollHub : Hub
{
    public async Task JoinPoll(string code)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, code);
    }

    public async Task LeavePoll(string code)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, code);
    }
}