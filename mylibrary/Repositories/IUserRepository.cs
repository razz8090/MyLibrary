using MongoDB.Driver;
using mylibrary.Models;
using mylibrary.Models.User;

namespace mylibrary.Repositories.Interfaces;

public interface IUserRepository
{
    Task<List<User>> GetAllUserAsync();
    Task AddUserAsync(User book);
    Task UpdateUserAsync(UpdateDefinition<User> updateDefinition, FilterDefinition<User> filterDefinition);
    Task<User> GetByIdAsync(FilterDefinition<User> filterDefinition);   
}

