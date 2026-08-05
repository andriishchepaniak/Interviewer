using Interviewer.Contracts.Models;
using Interviewer.Contracts.Requests;
using System.Text;

namespace Interviewer.Clients;

public class InterviewClientService
{
    const string ApiName = "InterviewApi";
    private readonly IHttpClientFactory _httpClientFactory;
    private const string BaseUrl = "api/interviews";

    public InterviewClientService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // GET api/interviews
    public async Task<List<Interview>?> GetAllAsync()
    {
        var client = _httpClientFactory.CreateClient(ApiName);

        return await client.GetFromJsonAsync<List<Interview>>(BaseUrl);
    }

    // GET api/interviews/{id}
    public async Task<Interview?> GetByIdAsync(string id)
    {
        var client = _httpClientFactory.CreateClient(ApiName);

        return await client.GetFromJsonAsync<Interview>($"{BaseUrl}/{id}");
    }

    // POST api/interviews
    public async Task<Interview?> CreateAsync(Interview interview)
    {
        var client = _httpClientFactory.CreateClient(ApiName);

        var response = await client.PostAsJsonAsync(BaseUrl, interview);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Interview>();
    }

    // PUT api/interviews/{id}
    public async Task UpdateAsync(string id, Interview updatedInterview)
    {
        var client = _httpClientFactory.CreateClient(ApiName);

        var response = await client.PutAsJsonAsync($"{BaseUrl}/{id}", updatedInterview);
        response.EnsureSuccessStatusCode();
    }

    // DELETE api/interviews/{id}
    public async Task DeleteAsync(string id)
    {
        var client = _httpClientFactory.CreateClient(ApiName);

        var response = await client.DeleteAsync($"{BaseUrl}/{id}");
        response.EnsureSuccessStatusCode();
    }

    // POST api/interviews/plan
    // Note: Replace 'InterviewPlan' with your actual return type model, 
    // or use 'string' if the backend returns plain text.
    public async Task<Topic[]?> GeneratePlanAsync(InterviewRequest request)
    {
        var client = _httpClientFactory.CreateClient(ApiName);

        var response = await client.PostAsJsonAsync($"{BaseUrl}/plan", request);
        response.EnsureSuccessStatusCode();

        // Assuming the plan returns as JSON. If it returns plain text, use ReadAsStringAsync()
        return await response.Content.ReadFromJsonAsync<Topic[]>();
    }

    // POST api/interviews/feedback/stream
    public async IAsyncEnumerable<string> StreamFeedbackAsync(Interview interview)
    {
        var client = _httpClientFactory.CreateClient(ApiName);

        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/feedback/stream")
        {
            Content = JsonContent.Create(interview)
        };

        // IMPORTANT FOR BLAZOR WEBASSEMBLY: 
        // This extension enables streaming for the underlying fetch API in the browser.
        // Uncomment the line below if you are using Blazor WebAssembly.
        // request.SetBrowserResponseStreamingEnabled(true);

        // Send the request and tell HttpClient to return as soon as headers are read (don't wait for full body)
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        // Read the stream as it comes in
        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream, Encoding.UTF8);

        char[] buffer = new char[1024];
        int bytesRead;

        // Read chunk by chunk until the stream is closed
        while ((bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            yield return new string(buffer, 0, bytesRead);
        }
    }
}
