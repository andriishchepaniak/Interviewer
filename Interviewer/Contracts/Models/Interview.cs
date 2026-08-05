namespace Interviewer.Contracts.Models;

public sealed class Interview
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CandidateName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string CvText { get; set; } = string.Empty;
    public List<Topic> Topics { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? FinalAiReport { get; set; }
}
