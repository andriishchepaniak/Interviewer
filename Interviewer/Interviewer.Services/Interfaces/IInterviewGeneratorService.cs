using Interviewer.Data.Models;

namespace Interviewer.Services;

public interface IInterviewGeneratorService
{
    Task<List<Topic>> GenerateInterviewPlanAsync(string position, string cvText);

    IAsyncEnumerable<string> GenerateFeedbackAsync(Interview state);
}
