using Interviewer.Contracts.Models;

namespace Interviewer.State;

public class InterviewState
{
    public Interview Current { get; set; } = new Interview();

    public bool IsGeneratingRequested { get; set; } = false;

    // true — Current вже збережений у базі. Наступне збереження має бути Update, а не Create,
    // інакше кожен повторний вхід у /review створює дублікат документа.
    public bool IsPersisted { get; set; } = false;

    public void Reset()
    {
        Current = new Interview();
        IsGeneratingRequested = false;
        IsPersisted = false;
    }
}
