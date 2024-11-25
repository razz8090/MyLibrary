using Microsoft.Extensions.Options;
using MongoDB.Driver;
using mylibrary.Config;
using mylibrary.Models.User;
using mylibrary.Repositories.Interfaces;

namespace mylibrary.Repositories.Services;

public class UserRepository: IUserRepository
{
    private readonly IMongoCollection<User> _user;
    public UserRepository(IOptions<MongoDbConfig> config, IMongoClient client)
	{
        var database = client.GetDatabase(config.Value.DatabaseName);
        _user = database.GetCollection<User>(nameof(User));
    }
    public async Task<List<User>> GetAllUserAsync() => await _user.Find(Builders<User>.Filter.Ne(x=>x.Status, Models.CommonModel.Status.Delete)).ToListAsync();
    public async Task AddUserAsync(User book) => await _user.InsertOneAsync(book);
    public async Task UpdateUserAsync(UpdateDefinition<User> updateDefinition, FilterDefinition<User> filterDefinition) => await _user.UpdateOneAsync(filterDefinition, updateDefinition);
    public async Task<User> GetByIdAsync(FilterDefinition<User> filterDefinition) => await _user.Find(filterDefinition).FirstOrDefaultAsync();
}

