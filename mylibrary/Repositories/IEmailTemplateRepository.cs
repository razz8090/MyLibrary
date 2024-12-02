using MongoDB.Driver;
using mylibrary.Models;

namespace mylibrary.Repositories;

public interface IEmailTemplateRepository
{
    public Task<EmailTemplate> GetByIdAsync(FilterDefinition<EmailTemplate> filterDefinition);

}

