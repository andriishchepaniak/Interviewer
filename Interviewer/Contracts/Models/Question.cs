namespace Interviewer.Contracts.Models;

public sealed class Question
{
    public string Text { get; set; } = string.Empty;
    public string Hint { get; set; } = string.Empty;

    // -1 = Skipped, 0-3 = Score, null = Not touched
    public int? Score { get; set; }
    public string Comment { get; set; } = string.Empty;
}
