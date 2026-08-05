using Interviewer.Contracts.Models;
using Interviewer.Infrastructure.Interfaces;
using Interviewer.Services.Options;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Interviewer.Services;

public class InterviewGeneratorService : IInterviewGeneratorService
{
    private readonly IAIService _aiService;
    private readonly PromptOptions _promptOptions;

    public InterviewGeneratorService(IAIService aiService, IOptions<PromptOptions> promptOptions)
    {
        _aiService = aiService;
        _promptOptions = promptOptions.Value;
    }

    public async Task<Topic[]> GenerateInterviewPlanAsync(string position, string cvText)
    {
        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        string fullPath = Path.Combine(basePath, _promptOptions.InterviewPlanPath);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Could not find the prompt file at: {fullPath}");
        }

        // Read prompt from txt file
        string template = await File.ReadAllTextAsync(fullPath);

        // Replace placeholders
        string prompt = template
            .Replace("{{Position}}", position)
            .Replace("{{CvText}}", cvText);

        string rawResponse = await _aiService.GenerateTextAsync(prompt);

        if (string.IsNullOrWhiteSpace(rawResponse))
            return [];

        string cleanJson = CleanJson(rawResponse);

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var topics = JsonSerializer.Deserialize<Topic[]>(cleanJson, options);

            if (topics != null)
            {
                for (int i = 0; i < topics.Length; i++) topics[i].Id = i + 1;
                return topics;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JSON Parsing Error: {ex.Message}");
        }

        return [];
    }

    public async IAsyncEnumerable<string> GenerateFeedbackAsync(Interview interview)
    {
        var answeredData = interview.Topics.Select(t => new
        {
            Topic = t.Title,
            Questions = t.Questions.Where(q => q.Score >= 0).Select(q => new
            {
                Question = q.Text,
                Score = q.Score,
                Comment = q.Comment
            })
        }).Where(t => t.Questions.Any());

        string jsonData = JsonSerializer.Serialize(answeredData);

        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        string fullPath = Path.Combine(basePath, _promptOptions.FeedbackPath);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Could not find the prompt file at: {fullPath}");
        }

        // Read prompt from txt file
        string template = await File.ReadAllTextAsync(fullPath);
       
        // Replace placeholders
        string prompt = template
            .Replace("{{CandidateName}}", interview.CandidateName)
            .Replace("{{Position}}", interview.Position)
            .Replace("{{JsonData}}", jsonData);

        await foreach (var chunk in _aiService.GenerateTextStreamAsync(prompt))
        {
            if (!string.IsNullOrEmpty(chunk))
            {
                yield return chunk;
            }
        }
    }

    private string CleanJson(string aiResponse)
    {
        var result = aiResponse.Trim();
        if (result.StartsWith("```json")) result = result.Substring(7);
        if (result.StartsWith("```")) result = result.Substring(3);
        if (result.EndsWith("```")) result = result.Substring(0, result.Length - 3);
        return result.Trim();
    }
}