using PollingSurvey.Application.DTOs;

namespace PollingSurvey.Application.Interfaces;

public interface IPollNotifier
{
    Task BroadcastPollUpdateAsync(string pollCode, PollResultResponse result);
}