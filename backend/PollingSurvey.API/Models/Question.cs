namespace PollSurvey.API.Models;

public class Question
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PollId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Type { get; set; } = "multiple_choice"; // multiple_choice | yes_no | rating | open_text
    public int Order { get; set; }

    public Poll Poll { get; set; } = null!;
    public ICollection<Option> Options { get; set; } = new List<Option>();
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}