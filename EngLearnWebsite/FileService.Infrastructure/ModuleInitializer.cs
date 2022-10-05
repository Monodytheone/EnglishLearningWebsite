using FileService.Domain;
using FileService.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zack.Commons;

namespace FileService.Infrastructure
{
    internal class ModuleInitializer : IModuleInitializer
    {
        public void Initialize(IServiceCollection services)
        {
            services
                .AddScoped<IStorageClient, SMBStorageClient>()
                .AddScoped<IStorageClient, MockCloudStorageClient>()
                .AddScoped<IFSRepository, FSRepository>()
                .AddScoped<FSDomainService>()
                .AddHttpContextAccessor()
                .AddHttpClient();
        }
    }
}
