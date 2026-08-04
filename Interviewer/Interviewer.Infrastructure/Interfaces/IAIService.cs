namespace Interviewer.Infrastructure.Interfaces;

public interface IAIService
{
    Task<string> GenerateTextAsync(string prompt);

    IAsyncEnumerable<string> GenerateTextStreamAsync(string prompt);
}
