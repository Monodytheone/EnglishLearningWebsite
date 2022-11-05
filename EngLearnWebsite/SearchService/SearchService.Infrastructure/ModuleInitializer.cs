using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nest;
using SearchService.Domain;
using Zack.Commons;

namespace SearchService.Infrastructure;

internal class ModuleInitializer : IModuleInitializer
{
    public void Initialize(IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddScoped<IElasticClient>(serviceProvider =>
        {
            IOptions<ElasticSearchOptions> elasticSearchOptions =
                serviceProvider.GetRequiredService<IOptions<ElasticSearchOptions>>();
            Nest.ConnectionSettings settings = new(elasticSearchOptions.Value.Url);
            return new ElasticClient(settings);
        });
        services.AddScoped<ISearchRepository, SearchRepository>();
    }
}
