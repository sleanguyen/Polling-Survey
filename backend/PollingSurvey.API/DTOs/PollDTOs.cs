namespace PollSurvey.API.DTOs;

// --- REQUEST ---

public class CreatePollRequest
{
    public string Title { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public List<CreateQuestionRequest> Questions { get; set; } = new();
}

public class CreateQuestionRequest
{
    public string Text { get; set; } = string.Empty;
    public string Type { get; set; } = "multiple_choice"; // multiple_choice | yes_no | rating | open_text
    public int Order { get; set; }
    public List<CreateOptionRequest> Options { get; set; } = new();
}

public class CreateOptionRequest
{
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }
}

public class SubmitVoteRequest
{
    public Guid QuestionId { get; set; }
    public Guid? OptionId { get; set; }        // multiple_choice, yes_no
    public int? RatingValue { get; set; }      // rating
    public string? OpenTextValue { get; set; } // open_text
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