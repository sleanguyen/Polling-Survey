using PollingSurvey.Application.Common;
using PollingSurvey.Application.DTOs;

namespace PollingSurvey.Application.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<RegisterResponse>> RegisterAsync(RegisterRequest request);
}
