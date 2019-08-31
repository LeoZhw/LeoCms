﻿using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Leo.Microservice.Host
{
    public interface IServiceHostBuilder
    {
        IServiceHost Build();

        IServiceHostBuilder RegisterServices(Action<ContainerBuilder> builder);

        IServiceHostBuilder ConfigureLogging(Action<ILoggingBuilder> configure);

        IServiceHostBuilder ConfigureServices(Action<IServiceCollection> configureServices);

        IServiceHostBuilder Configure(Action<IConfigurationBuilder> builder);

        IServiceHostBuilder MapServices(Action<IContainer> mapper);
    }
}
