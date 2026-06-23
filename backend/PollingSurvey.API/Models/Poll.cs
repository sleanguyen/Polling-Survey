namespace PollSurvey.API.Models;

public class Poll
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;      // short code: 7fGh2
    public string Title { get; set; } = string.Empty;     // tiêu đề khảo sát
    public string Status { get; set; } = "open";          // open | closed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }               // null = không hết hạn

    public ICollection<Question> Questions { get; set; } = new List<Question>();
}