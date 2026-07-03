using Microsoft.AspNetCore.Mvc;
using PollingSurvey.Application.Common;
using PollingSurvey.Application.DTOs;
using PollingSurvey.Application.Interfaces;

namespace PollingSurvey.API.Controllers;

[ApiController]
[Route("api/polls")]
public class PollsController : ControllerBase
{
    private readonly IPollService _pollService;

    public PollsController(IPollService pollService)
    {
        _pollService = pollService;
    }

    // POST api/polls
    [HttpPost]
    public async Task<IActionResult> CreatePoll([FromBody] CreatePollRequest request)
    {
        var result = await _pollService.CreatePollAsync(request);

        return result.Status switch
        {
            ServiceResultStatus.ValidationError => BadRequest(new { message = result.Message }),
            ServiceResultStatus.Success => CreatedAtAction(nameof(GetPoll), new { code = result.Data!.Code }, result.Data),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new { message = "Unexpected error." })
        };
    }

    // GET api/polls/{code}
    [HttpGet("{code}")]
    public async Task<IActionResult> GetPoll(string code)
    {
        var result = await _pollService.GetPollAsync(code);

        return result.Status switch
        {
            ServiceResultStatus.NotFound => NotFound(new { message = result.Message }),
            ServiceResultStatus.Success => Ok(result.Data),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new { message = "Unexpected error." })
        };
    }

    // POST api/polls/{code}/vote
    [HttpPost("{code}/vote")]
    public async Task<IActionResult> SubmitVote(string code, [FromBody] SubmitVoteRequest request)
    {
        var result = await _pollService.SubmitVoteAsync(code, request);

        return result.Status switch
        {
            ServiceResultStatus.NotFound => NotFound(new { message = result.Message }),
            ServiceResultStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, new { message = result.Message }),
            ServiceResultStatus.Conflict => Conflict(new { message = result.Message }),
            ServiceResultStatus.Success => Ok(new { message = result.Data }),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new { message = "Unexpected error." })
        };
    }

    // GET api/polls/{code}/results
    [HttpGet("{code}/results")]
    public async Task<IActionResult> GetResults(string code)
    {
        var result = await _pollService.GetResultsAsync(code);

        return result.Status switch
        {
            ServiceResultStatus.NotFound => NotFound(new { message = result.Message }),
            ServiceResultStatus.Success => Ok(result.Data),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new { message = "Unexpected error." })
        };
    }

    // PATCH api/polls/{code}/close
    [HttpPatch("{code}/close")]
    public async Task<IActionResult> ClosePoll(string code)
    {
        var result = await _pollService.ClosePollAsync(code);

        return result.Status switch
        {
            ServiceResultStatus.NotFound => NotFound(new { message = result.Message }),
            ServiceResultStatus.ValidationError => BadRequest(new { message = result.Message }),
            ServiceResultStatus.Success => Ok(new { message = result.Data }),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new { message = "Unexpected error." })
        };
    }
}