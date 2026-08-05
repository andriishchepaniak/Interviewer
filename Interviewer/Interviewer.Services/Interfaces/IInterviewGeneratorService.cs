using Interviewer.Contracts.Models;

namespace Interviewer.Services;

public interface IInterviewGeneratorService
{
    Task<Topic[]> GenerateInterviewPlanAsync(string position, string cvText);

    IAsyncEnumerable<string> GenerateFeedbackAsync(Interview state);
}
