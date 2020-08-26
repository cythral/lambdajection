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

### Lambda Handler

Writing the lambda is simple: Just define a public, partial class that contains a Handle method and annotate the class with the `Lambda` attribute. The `Lambda` attribute requires that you specify a startup class - more on this in the next step. You are not limited to an request/input parameter of type object - this can be any serializable value or reference type. Same goes for the return value, however the return value must be enclosed in a `Task`.

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

        public Task<object> Handle(object request, ILambdaContext context)
        {
            return new {
                foo = request
            };
        }
    }
}

```

### Startup Configuration

The startup class configures services that are injected into the Lambda's IoC container / service collection.

- Use the ConfigureServices method to add services to your lambda.
  - Use `IServiceCollection.UseAwsService<IAmazonService>` to inject AWS Clients and Client Factories into the lambda. See [the example here.](examples/AwsClientFactories)
- Optionally use the ConfigureLogging method to configure additional log settings.

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
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // configure injected services here
            services.AddScoped<IRegisteredService, DefaultRegisteredService>();

            // Add AWS Services by their interface name - the default
            services.UseAwsService<IAmazonService>();
        }

        public void ConfigureLogging(ILoggingBuilder logging)
        {
            // this method is optional
            // logging comes preconfigured to log to the console
        }
    }
}
```

### Adding Options

You can add an options section by defining a class for that section, and annotating it with the [LambdaOptions attribute](src/Attributes/LambdaOptionsAttribute.cs). If any options are in encrypted form, add the [Encrypted attribute](src/Encryption/EncryptedAttribute.cs) to that property. When the options are requested, the [IDecryptionService](src/Encryption/IDecryptionService.cs) singleton in the container will be used to decrypt those properties. The [default decryption service](src/Encryption/DefaultDecryptionService.cs) uses KMS to decrypt values.

- The Encrypted attribute, IDecryptionService and DefaultDecryptionService are all provided by the `Lambdajection.Encryption` package.
- Option classes must be in the same assembly as your lambda.
- You can replace the default decryption service with your own `IDecryptionService` by injecting it as a singleton in your Startup class' `ConfigureServices` method.
- See [the example for using Encrypted properties.](examples/EncryptedOptions)

```cs
using Lambdajection.Encryption;

namespace Your.Namespace
{
    [LambdaOptions(typeof(LambdaHandler), "SectionName")]
    public class ExampleOptions
    {
        public string ExampleValue { get; set; }

        [Encrypted]
        public string ExampleEncryptedValue { get; set; }
    }
}
```

### Handler Scheme

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
