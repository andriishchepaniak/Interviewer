using Interviewer.Contracts.Models;

namespace Interviewer.Services.Mappers;

public static class InterviewMapper
{
    extension(Interview interview)
    {
        public Data.Models.Interview ToEntity()
        {
            return new Data.Models.Interview
            {
                Id = interview.Id,
                CandidateName = interview.CandidateName,
                Position = interview.Position,
                CvText = interview.CvText,
                Topics = interview.Topics.Select(t => t.ToEntity()).ToArray(),
                CreatedAt = interview.CreatedAt,
                FinalAiReport = interview.FinalAiReport
            };
        }
    }

    extension(Data.Models.Interview interview)
    {
        public Interview ToResponse()
        {
            return new Interview
            {
                Id = interview.Id,
                CandidateName = interview.CandidateName,
                Position = interview.Position,
                CvText = interview.CvText,
                Topics = interview.Topics.Select(t => t.ToResponse()).ToList(),
                CreatedAt = interview.CreatedAt,
                FinalAiReport = interview.FinalAiReport
            };
        }
    }

    extension(Topic topic)
    {
        public Data.Models.Topic ToEntity()
        {
            return new Data.Models.Topic
            {
                Id = topic.Id,
                Title = topic.Title,
                Questions = topic.Questions.Select(q => q.ToEntity()).ToArray()
            };
        }
    }

    extension(Data.Models.Topic topic)
    {
        public Topic ToResponse()
        {
            return new Topic
            {
                Id = topic.Id,
                Title = topic.Title,
                Questions = topic.Questions.Select(q => q.ToResponse()).ToList()
            };
        }
    }

    extension(Question question)
    {
        public Data.Models.Question ToEntity()
        {
            return new Data.Models.Question
            {
                Text = question.Text,
                Hint = question.Hint,
                Score = question.Score,
                Comment = question.Comment
            };
        }
    }

    extension(Data.Models.Question question)
    {
        public Question ToResponse()
        {
            return new Question
            {
                Text = question.Text,
                Hint = question.Hint,
                Score = question.Score,
                Comment = question.Comment
            };
        }
    }
}
