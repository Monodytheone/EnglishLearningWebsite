using Listening.Domain.Entities.ValueObjects.SubtitleParser;
using Microsoft.Extensions.DependencyInjection;
using Zack.Commons;

namespace Listening.Domain
{
    /// <summary>
    /// 注册本项目所提供的服务
    /// </summary>
    internal class ModuleInitializer : IModuleInitializer
    {
        public void Initialize(IServiceCollection services)
        {
            services.AddScoped<ListeningDomainService>();
        }
    }
}
