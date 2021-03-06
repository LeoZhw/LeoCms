﻿using Autofac;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Leo.Microservice.Host
{
    public class ServiceHost : IServiceHost
    {
        private readonly ContainerBuilder _builder;
        private IStartup _startup;
        private IContainer _applicationServices;
        private readonly IServiceProvider _hostingServiceProvider;
        private readonly List<Action<IContainer>> _mapServicesDelegates;

        public ServiceHost(ContainerBuilder builder,
            IServiceProvider hostingServiceProvider,
             List<Action<IContainer>> mapServicesDelegate)
        {
            _builder = builder;
            _hostingServiceProvider = hostingServiceProvider;
            _mapServicesDelegates = mapServicesDelegate;
        }

        public void Dispose()
        {
            (_hostingServiceProvider as IDisposable)?.Dispose();
        }

        public IDisposable Run()
        {
            RunAsync().GetAwaiter().GetResult();
            return this;
        }

        public async Task RunAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_applicationServices != null)
                MapperServices(_applicationServices);
        }

        public async Task StopAsync(CancellationToken cancellationToken = default(CancellationToken))
        {

            using (var cts = new CancellationTokenSource(2000))
            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken))
            {
                var token = linkedCts.Token;
            }
        }

        public IContainer Initialize()
        {
            if (_applicationServices == null)
            {
                _applicationServices = BuildApplication();
            }
            return _applicationServices;
        }

        private IContainer BuildApplication()
        {
            try
            {
                EnsureApplicationServices();
                Action<IContainer> configure = _startup.Configure;
                if (_applicationServices == null)
                    _applicationServices = _builder.Build();
                configure(_applicationServices);
                return _applicationServices;
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("应用程序启动异常: " + ex.ToString());
                throw;
            }
        }

        private void EnsureApplicationServices()
        {
            if (_applicationServices == null)
            {
                EnsureStartup();
                _applicationServices = _startup.ConfigureServices(_builder);
            }
        }

        private void EnsureStartup()
        {
            if (_startup != null)
            {
                return;
            }

            _startup = _hostingServiceProvider.GetRequiredService<IStartup>();
        }

        

        private void MapperServices(IContainer mapper)
        {
            foreach (var mapServices in _mapServicesDelegates)
            {
                mapServices(mapper);
            }
        }
    }
}
