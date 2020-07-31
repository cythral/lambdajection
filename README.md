# Lambdajection
Write elegant and testable AWS Lambdas using .NET Core and Microsoft's Dependency Injection + Configuration extensions.  No longer do you need to write your own boilerplate to achieve this - just write your Lambda code and service configuration! 
 
## Installation
NuGet package coming soon

## Usage

### Lambda Class

```cs
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Lambdajection.Attributes;

namespace Your.Namespace
{
    [Lambda(Startup = typeof(Startup))]
    public class YourLambda
    {
        private IRegisteredService yourService;

        public YourLambda(IRegisteredService yourService)
        {
            this.yourService = yourService;
        }

        public Task<object> Handle(object bar, ILambdaContext context)
        {
            return new {
                foo = bar
            };
        }
    }
}

```

### Startup Class

```cs
using System;

using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Your.Namespace
{
    public class Startup : ILambdaStartup
    {
        public IConfiguration Configuration { get; set; } = default!;

        public void ConfigureServices(IServiceCollection services)
        {
            // configure injected services here
            services.AddScoped<IRegisteredService, DefaultRegisteredService>();
        }
    }
}
```