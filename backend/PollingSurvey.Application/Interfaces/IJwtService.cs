using PollingSurvey.Domain.Entities;

namespace PollingSurvey.Application.Interfaces;

public interface IJwtService
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}