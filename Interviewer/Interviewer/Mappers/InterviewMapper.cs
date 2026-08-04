using Interviewer.Data.Models;
using Interviewer.State;

namespace Interviewer.Mappers;

public static class InterviewMapper
{
    extension(InterviewState state)
    {
        public Interview ToInterview()
        {
            return state.Current;
        }
    }
}
