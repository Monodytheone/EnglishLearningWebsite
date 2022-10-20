using Listening.Domain;
using Microsoft.Extensions.DependencyInjection;
using Zack.Commons;

namespace Listening.Infrastructure
{
    internal class ModuleInitializer : IModuleInitializer
    {
        public void Initialize(IServiceCollection services)
        {
            services.AddScoped<IListeningRepository, ListeningRepository>();
        }
    }
}
