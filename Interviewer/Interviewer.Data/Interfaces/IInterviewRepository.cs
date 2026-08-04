using Interviewer.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Interviewer.Data.Interfaces;

public interface IInterviewRepository
{
    Task<List<Interview>> GetAllAsync();

    Task<Interview?> GetByIdAsync(string id);

    Task CreateAsync(Interview interview);

    Task UpdateAsync(string id, Interview interview);

    Task DeleteAsync(string id);
}
