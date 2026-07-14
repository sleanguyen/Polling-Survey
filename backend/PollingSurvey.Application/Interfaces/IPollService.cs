using PollingSurvey.Application.Common;
using PollingSurvey.Application.DTOs;

namespace PollingSurvey.Application.Interfaces;

public interface IPollService
{
    Task<ServiceResult<PollResponse>> CreatePollAsync(CreatePollRequest request);
    Task<ServiceResult<PollResponse>> GetPollAsync(string code);
    Task<ServiceResult<string>> SubmitVoteAsync(string code, SubmitVoteRequest request);
    Task<ServiceResult<PollResultResponse>> GetResultsAsync(string code);
    Task<ServiceResult<string>> ClosePollAsync(string code);
}