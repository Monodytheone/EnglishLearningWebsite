﻿using MediaEncoder.Domain;
using Microsoft.Extensions.DependencyInjection;
using Zack.Commons;

namespace MediaEncoder.Infrastructure
{
    public class ModuleInitializer : IModuleInitializer
    {
        public void Initialize(IServiceCollection services)
        {
            services.AddScoped<IMediaEncoder, ToM4AEncoder>();
            services.AddScoped<IMediaEncoderRepository, MediaEncoderRepository>();
        }
    }
}
