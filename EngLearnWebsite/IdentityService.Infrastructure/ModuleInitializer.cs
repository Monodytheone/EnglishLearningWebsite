using IdentityService.Domain;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Zack.Commons;

namespace IdentityService.Infrastructure;

internal class ModuleInitializer : IModuleInitializer
{
    public void Initialize(IServiceCollection services)
    {
        services.AddScoped<IIdRepository, IdRepository>();
        services.AddScoped<ISmsSender, MockSmsSender>();
        services.AddScoped<IdDomainService>();
        services.AddMemoryCache();
    }
}
