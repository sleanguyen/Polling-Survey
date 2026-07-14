using System.ComponentModel.DataAnnotations;

namespace PollingSurvey.Application.DTOs;

// --- REQUEST ---

public class CreatePollRequest
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title must not exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    public DateTime? ExpiresAt { get; set; }

    [Required(ErrorMessage = "At least one question is required.")]
    [MinLength(1, ErrorMessage = "At least one question is required.")]
    public List<CreateQuestionRequest> Questions { get; set; } = new();
}

public class CreateQuestionRequest
{
    [Required(ErrorMessage = "Question text is required.")]
    [StringLength(500, ErrorMessage = "Question text must not exceed 500 characters.")]
    public string Text { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = "multiple_choice"; // multiple_choice | yes_no | rating | open_text

    public int Order { get; set; }

    public List<CreateOptionRequest> Options { get; set; } = new();
}

public class CreateOptionRequest
{
    [Required(ErrorMessage = "Option text is required.")]
    [StringLength(200, ErrorMessage = "Option text must not exceed 200 characters.")]
    public string Text { get; set; } = string.Empty;

    public int Order { get; set; }
}

public class SubmitVoteRequest
{
    [Required(ErrorMessage = "QuestionId is required.")]
    public Guid QuestionId { get; set; }

    public Guid? OptionId { get; set; }        // multiple_choice, yes_no
    public int? RatingValue { get; set; }      // rating
    public string? OpenTextValue { get; set; } // open_text

    [Required(ErrorMessage = "VoterToken is required.")]
    public string VoterToken { get; set; } = string.Empty;
}

// --- RESPONSE ---

public class PollResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public List<QuestionResponse> Questions { get; set; } = new();
}

public class QuestionResponse
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Order { get; set; }
    public List<OptionResponse> Options { get; set; } = new();
}

public class OptionResponse
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }
}

public class PollResultResponse
{
    public Guid PollId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<QuestionResultResponse> Questions { get; set; } = new();
}

public class QuestionResultResponse
{
    public Guid QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int TotalVotes { get; set; }
    public List<OptionResultResponse> Options { get; set; } = new();
    public List<string> OpenTextAnswers { get; set; } = new();
    public double? AverageRating { get; set; }
}

public class OptionResultResponse
{
    public Guid OptionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int VoteCount { get; set; }
    public double Percentage { get; set; }
}