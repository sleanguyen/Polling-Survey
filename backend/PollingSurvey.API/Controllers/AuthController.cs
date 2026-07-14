using Microsoft.AspNetCore.Mvc;
using PollingSurvey.Application.Common;
using PollingSurvey.Application.DTOs;
using PollingSurvey.Application.Interfaces;

namespace PollingSurvey.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // POST api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        return result.Status switch
        {
            ServiceResultStatus.ValidationError => BadRequest(new { message = result.Message }),
            ServiceResultStatus.Conflict => Conflict(new { message = result.Message }),
            ServiceResultStatus.Success => StatusCode(StatusCodes.Status201Created, result.Data),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new { message = "Unexpected error." })
        };
    }
}
