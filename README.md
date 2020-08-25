# Lambdajection

Write elegant and testable AWS Lambdas using .NET Core and Microsoft's Dependency Injection + Configuration extensions. No longer do you need to write your own boilerplate to achieve this - just write your Lambda code and service configuration!

## Installation

```
dotnet add package Lambdajection
```

## Packages

|                          |                                                                                     |
| ------------------------ | ----------------------------------------------------------------------------------- |
| Lambdajection            | ![Nuget](https://img.shields.io/nuget/v/Lambdajection?style=flat-square)            |
| Lambdajection.Attributes | ![Nuget](https://img.shields.io/nuget/v/Lambdajection.Attributes?style=flat-square) |
| Lambdajection.Core       | ![Nuget](https://img.shields.io/nuget/v/Lambdajection.Core?style=flat-square)       |
| Lambdajection.Generator  | ![Nuget](https://img.shields.io/nuget/v/Lambdajection.Generator?style=flat-square)  |
| Lambdajection.Encryption | ![Nuget](https://img.shields.io/nuget/v/Lambdajection.Encryption?style=flat-square) |

## Usage

### Lambda Class

```cs
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Lambdajection.Attributes;

namespace Your.Namespace
{
    [Lambda(Startup = typeof(Startup))]
    public partial class YourLambda
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
using Microsoft.Extensions.Logging;

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

        public void ConfigureLogging(ILoggingBuilder logging)
        {
            // this method is optional
            // logging comes preconfigured to log to the console
        }
    }
}
```

### Lambda Handler

When configuring your lambda on AWS, the method name you'll want to use will be `Run` (NOT `Handle`). For context, `Run` is a static method is generated on your class during compilation that takes care of setting up the IoC container (if it hasn't been already).

So, going off the example above, the handler scheme would look like this:

```
Your.Assembly.Name::Your.Namespace.YourLambda::Run
```

## Examples

- [Injecting and using AWS Services + Factories](examples/AwsClientFactories)
- [Adding and using encrypted options](examples/EncryptedOptions)

## Acknowledgments

1. [CodeGeneration.Roslyn](https://github.com/aarnott/codegeneration.roslyn) - Used for compile-time code generation using attributes.
2. [Simple Lambda Dependency Injection in AWS Lambda .NET Core](https://dev.to/gary_woodfine/simple-dependency-injection-in-aws-lambda-net-core-n0g) by Gary Woodfine - primary inspiration for this project.

## License

This project is licensed under the [MIT License](LICENSE.txt).
