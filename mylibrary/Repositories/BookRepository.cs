using System;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using mylibrary.Config;
using mylibrary.Models;
using mylibrary.Repositories.Interfaces;

namespace mylibrary.Repositories.Services;

public class BookRepository: IBookRepository
{
    private readonly IMongoCollection<Book> _books;
    public BookRepository(IOptions<MongoDbConfig> config, IMongoClient client)
	{
        var database = client.GetDatabase(config.Value.DatabaseName);
        _books = database.GetCollection<Book>("Books");
    }

    public async Task<List<Book>> GetAllBooksAsync() => await _books.Find(_ => true).ToListAsync();
    public async Task AddBookAsync(Book book) => await _books.InsertOneAsync(book);
}

