using Interviewer.Contracts.Models;

namespace Interviewer.Services.Interfaces;

public interface IInterviewService
{
    Task<Interview[]> GetAllAsync();

    Task<Interview?> GetByIdAsync(string id);

    Task CreateAsync(Interview interview);

    Task UpdateAsync(string id, Interview interview);

    Task DeleteAsync(string id);
}
