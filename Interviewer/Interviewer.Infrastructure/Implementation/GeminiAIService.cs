using Google.GenAI;
using Interviewer.Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Interviewer.Infrastructure.Gemini;

public class GeminiAIService : IAIService
{
    private readonly IConfiguration _configuration;
    private readonly Client _client;

    public GeminiAIService(IConfiguration configuration)
    {
        _configuration = configuration;
        var apiKey = _configuration["GeminiApiKey"] ?? throw new InvalidOperationException("API Key is missing!");
        _client = new Client(apiKey: apiKey);
    }

    public async Task<string> GenerateTextAsync(string prompt)
    {
        try
        {
            var response = await _client.Models.GenerateContentAsync(
                model: "gemini-3-flash-preview", // Або "gemini-2.5-flash", якщо SDK підтримує
                contents: prompt
            );

            return response.Candidates[0].Content.Parts[0].Text;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in AIService: {ex.Message}");
            return string.Empty;
        }
    }

    public async IAsyncEnumerable<string> GenerateTextStreamAsync(string prompt)
    {
        var responseStream = _client.Models.GenerateContentStreamAsync(
            model: "gemini-3-flash-preview",
            contents: prompt
        );

        await foreach (var response in responseStream)
        {
            if (response.Candidates != null && response.Candidates.Count > 0)
            {
                yield return response.Candidates[0].Content.Parts[0].Text;
            }
        }
    }
}
