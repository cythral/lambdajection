<!-- omit in toc -->
# Lambdajection
[![Nuget](https://img.shields.io/nuget/v/Lambdajection?label=version&style=flat-square)](https://nuget.org/packages/Lambdajection) [![Nuget](https://img.shields.io/nuget/vpre/Lambdajection?label=pre-release&style=flat-square&color=blueviolet)](https://nuget.org/packages/Lambdajection) [![GitHub](https://img.shields.io/github/license/cythral/lambdajection?style=flat-square&color=lightgrey)](./LICENSE.txt) [![GitHub Workflow Status](https://img.shields.io/github/workflow/status/cythral/lambdajection/Continuous%20Integration?style=flat-square)](https://github.com/cythral/lambdajection/actions?query=workflow%3A%22Continuous+Integration%22) [![Sponsor on Github](https://img.shields.io/badge/sponsor-on%20github-pink?style=flat-square)](https://github.com/sponsor/cythral) [![Donate on Paypal](https://img.shields.io/badge/donate-on%20paypal-blue?style=flat-square)](https://paypal.me/cythral)

Write elegant and testable AWS Lambdas using .NET Core and Microsoft's Dependency Injection + Configuration extensions. No longer do you need to write your own boilerplate to achieve this - just write your Lambda code and service configuration! 

Lambdajection aims to:
- Facilitate rapid and secure lambda development using C#.
- Increase Lambda testability by enabling use of Microsoft's [dependency injection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection), [configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/) and [logging](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/) extensions. 
- Achieve faster startup times (and lower costs) by using compile-time code generation rather than reflection wherever possible.
- Optimize for container reuse by reusing service collections and services between invocations.
- Be highly extensible and configurable.

Community contribution/pull requests are welcome and encouraged! See the [contributing guide](CONTRIBUTING.md) for instructions. Report issues on [JIRA](https://cythral.atlassian.net/jira/software/c/projects/LAMBJ/issues) - you can report anonymously or include github username/contact info in the ticket summary.

<!-- omit in toc -->
## Table of Contents

- [1. Installation](#1-installation)
  - [1.1. Metapackage](#11-metapackage)
  - [1.2. Templates](#12-templates)
  - [1.3. Development Builds](#13-development-builds)
- [2. Packages](#2-packages)
- [3. Templates](#3-templates)
- [4. Usage](#4-usage)
  - [4.1. Lambda Handler](#41-lambda-handler)
  - [4.2. Serialization](#42-serialization)
  - [4.3. Startup Class](#43-startup-class)
  - [4.4. Customizing Configuration](#44-customizing-configuration)
  - [4.5. Adding Options](#45-adding-options)
  - [4.6. Initialization Services](#46-initialization-services)
  - [4.7. Handler Scheme](#47-handler-scheme)
- [5. Examples](#5-examples)
- [6. Acknowledgments](#6-acknowledgments)
- [7. Donations](#7-donations)
- [8. Contributing](#8-contributing)
- [9. Security](#9-security)
- [10. License](#10-license)

## 1. Installation

### 1.1. Metapackage
See the [packages](#2-packages) section for a list of available packages.

```
dotnet add package Lambdajection
```

### 1.2. Templates
> ℹ Templates will be available starting in v0.5.0-beta1

```
dotnet new -i Lambdajection.Templates
```

### 1.3. Development Builds

Development builds are generated for PRs and uploaded to GitHub Packages.  To use them, update the **user** config file for nuget (varies by OS - see [this article](https://docs.microsoft.com/en-us/nuget/consume-packages/configuring-nuget-behavior)) and add this to the packageSourceCredentials section of that file:

```xml
<github>
    <add key="Username" value="USERNAME" />
    <add key="ClearTextPassword" value="TOKEN" />
</github>
```

Replace USERNAME with your username, and TOKEN with a personal access token from GitHub that has permissions to read packages.  It is important that this goes in the user config file rather than the project one, so that you do not accidentally leak your personal access token to the world.

Then, in your **project**'s nuget.config file, add the following to your packageSources section:

```xml
<add key="github" value="https://nuget.pkg.github.com/cythral/index.json" />
```

Finally, you may use development builds by adding the package and version to your .csproj, for instance:

```xml
<PackageReference Include="Lambdajection" Version="0.3.0-gc2ca768d3f" />
```

## 2. Packages

|                          |                                                                                                                                                                                                   |
| ------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Lambdajection            | ![Nuget](https://img.shields.io/nuget/v/Lambdajection?label=version&style=flat-square) ![Nuget](https://img.shields.io/nuget/dt/Lambdajection?color=blue&style=flat-square)                       |
| Lambdajection.Attributes | ![Nuget](https://img.shields.io/nuget/v/Lambdajection.Attributes?label=version&style=flat-square) ![Nuget](https://img.shields.io/nuget/dt/Lambdajection.Attributes?color=blue&style=flat-square) |
| Lambdajection.Core       | ![Nuget](https://img.shields.io/nuget/v/Lambdajection.Core?label=version&style=flat-square) ![Nuget](https://img.shields.io/nuget/dt/Lambdajection.Core?color=blue&style=flat-square)             |
| Lambdajection.Generator  | ![Nuget](https://img.shields.io/nuget/v/Lambdajection.Generator?label=version&style=flat-square) ![Nuget](https://img.shields.io/nuget/dt/Lambdajection.Generator?color=blue&style=flat-square)   |
| Lambdajection.Encryption | ![Nuget](https://img.shields.io/nuget/v/Lambdajection.Encryption?label=version&style=flat-square) ![Nuget](https://img.shields.io/nuget/dt/Lambdajection.Encryption?color=blue&style=flat-square) |
| Lambdajection.Templates  | ![Nuget](https://img.shields.io/nuget/v/Lambdajection.Templates?label=version&style=flat-square) ![Nuget](https://img.shields.io/nuget/dt/Lambdajection.Templates?color=blue&style=flat-square)   |

## 3. Templates
> ℹ Templates will be available starting in v0.5.0-beta1

Lambdajection .NET templates are available for your convenience.  Run `dotnet new [template-name] --help` to see a list of available options for each template.


<!-- omit in toc -->
### Lambdajection Project
```
dotnet new lambdajection
```
Creates a new C# project with Lambdajection installed, plus boilerplate for a Lambda Handler and Startup class.

<!-- omit in toc -->
### Options Class
```
dotnet new lambdajection-options
```
Creates a new Options class to be injected into your Lambda as an `IOption<>`.


## 4. Usage

### 4.1. Lambda Handler

Writing the lambda is simple: Just define a public, partial class that contains a Handle method and annotate the class with the `Lambda` attribute. The `Lambda` attribute requires that you specify a startup class - more on this in the next step. You are not limited to an request/input parameter of type object - this can be any serializable value or reference type. Same goes for the return value, however the return value must be enclosed in a `Task`.

```cs
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Lambdajection.Attributes;

namespace Your.Namespace
{
    [Lambda(typeof(Startup))]
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

### 4.2. Serialization

If your Lambda targets the `netcoreapp3.1` framework, then by default, the serializer is set to  `DefaultLambdaJsonSerializer` from the `Amazon.Lambda.Serialization.SystemTextJson` package.  With any TFM, you may specify the serializer you want to use by setting the Lambda attribute's `Serializer` argument:

```cs
[Lambda(typeof(Startup), Serializer = typeof(Serializer))]
public partial class Lambda
{
    ...
```

See a [full example of serializer customization](./examples/CustomSerializer/README.md). 


### 4.3. Startup Class

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

### 4.4. Customizing Configuration

By default, configuration is environment variables-based. If you would like to use a file-based or other configuration scheme, you may supply a custom configuration factory to the Lambda attribute:

```cs
[Lambda(typeof(Startup), ConfigFactory = typeof(ConfigFactory))]
public partial class Lambda
{
    ...
```

A custom config factory might look like the following:
```cs
using System.IO;

using Lambdajection.Core;

using Microsoft.Extensions.Configuration;

namespace Your.Namespace
{
    public class ConfigFactory : ILambdaConfigFactory
    {
        public IConfigurationRoot Create()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
        }
    }
}
```

### 4.5. Adding Options

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

### 4.6. Initialization Services
Initialization services can be used to initialize data or perform some task before the lambda is run.  Initialization services should implement [ILambdaInitializationService](src/Core/ILambdaInitializationService.cs) and be injected into the container as singletons at startup.

### 4.7. Handler Scheme

When configuring your lambda on AWS, the method name you'll want to use will be `Run` (NOT `Handle`). For context, `Run` is a static method is generated on your class during compilation that takes care of setting up the IoC container (if it hasn't been already).

So, going off the example above, the handler scheme would look like this:

```
Your.Assembly.Name::Your.Namespace.YourLambda::Run
```

## 5. Examples

- [Injecting and using AWS Services + Factories](examples/AwsClientFactories)
- [Automatic decryption of encrypted options](examples/EncryptedOptions)
- [Using a custom serializer](examples/CustomSerializer/README.md)

## 6. Acknowledgments

1. [CodeGeneration.Roslyn](https://github.com/aarnott/codegeneration.roslyn) - Used for compile-time code generation using attributes.
2. [Simple Lambda Dependency Injection in AWS Lambda .NET Core](https://dev.to/gary_woodfine/simple-dependency-injection-in-aws-lambda-net-core-n0g) by Gary Woodfine - primary inspiration for this project.

## 7. Donations

If you use this project and find it useful, please consider donating.  Donations are accepted via [Github Sponsors](https://github.com/sponsors/cythral) and [PayPal](https://www.paypal.com/paypalme/cythral).

## 8. Contributing

Issues and feature requests may be reported anonymously on [JIRA Cloud](https://cythral.atlassian.net/jira/software/c/projects/LAMBJ/issues).
Pull requests are always welcome! See the [contributing guide](CONTRIBUTING.md) for more information.

## 9. Security

Security issues can be reported on our JIRA.  See our [security policy](SECURITY.md) for more information. 

## 10. License

This project is licensed under the [MIT License](LICENSE.txt).
