using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lambdajection.Generator
{
    public class GeneratorHost : IHost
    {
        public GeneratorHost(IServiceProvider services)
        {
            Services = services;
        }

        public IServiceProvider Services { get; }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;

            var unitGenerator = Services.GetRequiredService<UnitGenerator>();
            unitGenerator.Generate();

            var lifetime = Services.GetRequiredService<IHostApplicationLifetime>();
            lifetime.StopApplication();
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
