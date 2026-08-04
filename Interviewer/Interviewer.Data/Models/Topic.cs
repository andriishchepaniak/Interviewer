namespace Interviewer.Data.Models;

public class Topic
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<Question> Questions { get; set; } = [];
}