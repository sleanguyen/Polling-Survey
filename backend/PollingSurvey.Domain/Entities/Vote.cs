namespace PollingSurvey.Domain.Entities;

public class Vote
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid QuestionId { get; set; }
    public Guid? OptionId { get; set; }        // null nếu là open_text hoặc rating
    public int? RatingValue { get; set; }      // dùng cho question type = rating
    public string? OpenTextValue { get; set; } // dùng cho question type = open_text
    public string VoterToken { get; set; } = string.Empty; // fingerprint/cookie để chặn vote lại
    public DateTime VotedAt { get; set; } = DateTime.UtcNow;

    public Question Question { get; set; } = null!;
    public Option? Option { get; set; }
}