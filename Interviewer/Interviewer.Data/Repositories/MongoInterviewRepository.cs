using Interviewer.Data.Interfaces;
using Interviewer.Data.Models;
using Interviewer.Data.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Interviewer.Data.Repositories;

public class MongoInterviewRepository : IInterviewRepository
{
    private readonly IMongoCollection<Interview> _interviewsCollection;

    public MongoInterviewRepository(IOptions<MongoDbSettings> mongoDbSettings)
    {
        var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);

        _interviewsCollection = mongoDatabase.GetCollection<Interview>(mongoDbSettings.Value.CollectionName);
    }

    public async Task<List<Interview>> GetAllAsync() =>
        await _interviewsCollection.Find(_ => true).ToListAsync();

    public async Task<Interview?> GetByIdAsync(string id) =>
        await _interviewsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Interview interview) =>
        await _interviewsCollection.InsertOneAsync(interview);

    public async Task UpdateAsync(string id, Interview interview) =>
        await _interviewsCollection.ReplaceOneAsync(x => x.Id == id, interview);

    public async Task DeleteAsync(string id) =>
        await _interviewsCollection.DeleteOneAsync(x => x.Id == id);
}