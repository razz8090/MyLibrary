using MongoDB.Driver;
using mylibrary.Models;
using mylibrary.Models.User;

namespace mylibrary.Repositories.Interfaces;

public interface IUserRepository
{
    Task<List<User>> GetAllBooksAsync();
    Task AddBookAsync(User book);
    Task UpdateBookAsync(UpdateDefinition<User> updateDefinition, FilterDefinition<User> filterDefinition);
    Task<User> GetByIdAsync(FilterDefinition<User> filterDefinition);
}

