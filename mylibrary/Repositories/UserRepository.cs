using Microsoft.Extensions.Options;
using MongoDB.Driver;
using mylibrary.Config;
using mylibrary.Models;
using mylibrary.Models.User;
using mylibrary.Repositories.Interfaces;

namespace mylibrary.Repositories.Services;

public class UserRepository: IUserRepository
{
    private readonly IMongoCollection<User> _user;
    private readonly IMongoCollection<BlockedToken> _blockToken;
    public UserRepository(IOptions<MongoDbConfig> config, IMongoClient client)
	{
        var database = client.GetDatabase(config.Value.DatabaseName);
        _user = database.GetCollection<User>(nameof(User));
        _blockToken = database.GetCollection<BlockedToken>(nameof(BlockedToken));
    }
    public async Task<List<User>> GetAllUserAsync() => await _user.Find(Builders<User>.Filter.Ne(x=>x.Status, Models.CommonModel.Status.Delete)).ToListAsync();
    public async Task AddUserAsync(User book) => await _user.InsertOneAsync(book);
    public async Task UpdateUserAsync(UpdateDefinition<User> updateDefinition, FilterDefinition<User> filterDefinition) => await _user.UpdateOneAsync(filterDefinition, updateDefinition);
    public async Task<User> GetByIdAsync(FilterDefinition<User> filterDefinition) => await _user.Find(filterDefinition).FirstOrDefaultAsync();
    public async Task ReplaceUserAsync(FilterDefinition<User> filterUser, User user) => await _user.ReplaceOneAsync(filterUser, user);
    public async Task AddBlockToken(BlockedToken newToken) => await _blockToken.InsertOneAsync(newToken);
    public async Task<BlockedToken> GetBlockTokenByToken(FilterDefinition<BlockedToken> filterToken) => await _blockToken.Find(filterToken).FirstOrDefaultAsync();
    public async Task RemoveExpireToken(FilterDefinition<BlockedToken> filterDefination) => await _blockToken.DeleteManyAsync(filterDefination);
}

