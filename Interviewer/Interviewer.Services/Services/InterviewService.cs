using Interviewer.Contracts.Models;
using Interviewer.Data.Interfaces;
using Interviewer.Services.Interfaces;
using Interviewer.Services.Mappers;

namespace Interviewer.Services.Services;

public class InterviewService(IInterviewRepository interviewRepository) : IInterviewService
{
    public async Task CreateAsync(Interview interview)
    {
        await interviewRepository.CreateAsync(interview.ToEntity());
    }

    public async Task DeleteAsync(string id)
    {
        await interviewRepository.DeleteAsync(id);
    }

    public async Task<Interview[]> GetAllAsync()
    {
        var interviews = await interviewRepository.GetAllAsync();

        return interviews.Select(i => i.ToResponse()).ToArray();
    }

    public async Task<Interview?> GetByIdAsync(string id)
    {
        var interview = await interviewRepository.GetByIdAsync(id);

        return interview?.ToResponse();
    }

    public async Task UpdateAsync(string id, Interview interview)
    {
        await interviewRepository.UpdateAsync(id, interview.ToEntity());
    }
}
