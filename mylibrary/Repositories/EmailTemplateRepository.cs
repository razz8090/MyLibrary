using Microsoft.Extensions.Options;
using MongoDB.Driver;
using mylibrary.Config;
using mylibrary.Models;

namespace mylibrary.Repositories;

public class EmailTemplateRepository: IEmailTemplateRepository
{

    private readonly IMongoCollection<EmailTemplate> _emailTemplate;
    public EmailTemplateRepository(IOptions<MongoDbConfig> config, IMongoClient client)
    {
        var database = client.GetDatabase(config.Value.DatabaseName);
        _emailTemplate = database.GetCollection<EmailTemplate>(nameof(EmailTemplate));
    }

    public async Task<EmailTemplate> GetByIdAsync(FilterDefinition<EmailTemplate> filterDefinition) => await _emailTemplate.Find(filterDefinition).FirstOrDefaultAsync();
}

