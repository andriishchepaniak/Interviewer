namespace Interviewer.Contracts.Models;

public sealed class Topic
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<Question> Questions { get; set; } = [];
}
