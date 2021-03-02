# Lambdajection
[![Nuget](https://img.shields.io/nuget/v/Lambdajection?label=version&style=flat-square)](https://nuget.org/packages/Lambdajection) [![Nuget](https://img.shields.io/nuget/vpre/Lambdajection?label=pre-release&style=flat-square&color=blueviolet)](https://nuget.org/packages/Lambdajection) [![GitHub](https://img.shields.io/github/license/cythral/lambdajection?style=flat-square&color=lightgrey)](./LICENSE.txt) [![GitHub Workflow Status](https://img.shields.io/github/workflow/status/cythral/lambdajection/Continuous%20Integration?style=flat-square)](https://github.com/cythral/lambdajection/actions?query=workflow%3A%22Continuous+Integration%22) [![Codecov](https://img.shields.io/codecov/c/github/cythral/lambdajection?style=flat-square&token=L8FWGHHVU1)](https://codecov.io/gh/cythral/lambdajection/) [![Sponsor on Github](https://img.shields.io/badge/sponsor-on%20github-pink?style=flat-square)](https://github.com/sponsors/cythral) [![Donate on Paypal](https://img.shields.io/badge/donate-on%20paypal-blue?style=flat-square)](https://paypal.me/cythral)

Write elegant and testable AWS Lambdas using C#/.NET and Microsoft's extensions for dependency injection, configuration and more. Start from a template, add handler code, inject services and let Lambdajection do the rest!

Why Lambdajection?
- Easy dependency management: configure dependencies/services using a Startup class, similar to how it is done in ASP.NET Core.
- Easy secrets management: automatically decrypt options marked as `[Encrypted]` using KMS or your own provided cryptography service.
- Faster startup times: we're using code generation over reflection wherever possible to help minimize cold-start times.
- Highly configurable: customize serialization, configuration, and run custom code before your main handler is invoked.
- Highly testable: facilitates use of dependency-injection to make testing your Lambda easier.
- Flexibility: you can use AWS' provided runtime or roll your own runtime containing .NET. Lambdajection works both ways, and can even generate code for running on a custom/provided runtime.

Community contribution/pull requests are welcome and encouraged! See the [contributing guide](CONTRIBUTING.md) for instructions. Report issues on [JIRA](https://cythral.atlassian.net/jira/software/c/projects/LAMBJ/issues) - you can report anonymously or include github username/contact info on the ticket.

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
  - [4.7. Disposers](#47-disposers)
  - [4.8. Handler Scheme](#48-handler-scheme)
  - [4.9. Custom Runtimes](#49-custom-runtimes)
  - [4.10. Lambda Layer](#410-lambda-layer)
- [5. Examples](#5-examples)
- [6. Acknowledgments](#6-acknowledgments)
- [7. Donations](#7-donations)
- [8. Contributing](#8-contributing)
- [9. Security](#9-security)
- [10. License](#10-license)

## 1. Installation

### 1.1. Metapackage
See the [packages](#2-packages) section for a list of available packages.  Starting in v0.5.0-beta2, you will need to have the .NET 5 SDK installed.

```
dotnet add package Lambdajection
```

### 1.2. Templates

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

Browse development builds [here](https://github.com/orgs/cythral/packages?repo_name=lambdajection).

## 2. Packages

|                                        |                                                                                                                                                                                                                               |
| -------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Lambdajection                          | ![Nuget](https://img.shields.io/nuget/v/Lambdajection?label=version&style=flat-square) ![Nuget](https://img.shields.io/nuget/dt/Lambdajection?color=blue&style=flat-square)                                                   |
| Lambdajection.Attributes               | ![Nuget](https://img.shields.io/nuget/v/Lambdajection.Attributes?label=version&style=flat-square) ![Nuget](https://img.shields.io/nuget/dt/Lambdajection.Attributes?color=blue&style=flat-square)                             |
| Lambdajection.Core                     | ![Nuget](https://img.shields.io/nuget/v/Lambdajection.Core?label=version&style=flat-square) ![Nuget](https://img.shields.io/nuget/dt/Lambdajection.Core?color=blue&style=flat-square)                                         |
| Lambdajection.Generator                | ![Nuget](https://img.shields.io/nuget/v/Lambdajection.Generator?label=version&style=flat-square) ![Nuget](https://img.shields.io/nuget/dt/Lambdajection.Generator?color=blue&style=flat-square)                               |
| Lambdajection.Encryption               | ![Nuget](https://img.shields.io/nuget/v/Lambdajection.Encryption?label=version&style=flat-square) ![Nuget](https://img.shields.io/nuget/dt/Lambdajection.Encryption?color=blue&style=flat-square)                             |
| Lambdajection.Templates                | ![Nuget](https://img.shields.io/nuget/v/Lambdajection.Templates?label=version&style=flat-square) ![Nuget](https://img.shields.io/nuget/dt/Lambdajection.Templates?color=blue&style=flat-square)                               |
| Lambdajection.Runtime                  | ![Nuget](https://img.shields.io/nuget/v/Lambdajection.Runtime?label=version&style=flat-square) ![Nuget](https://img.shields.io/nuget/dt/Lambdajection.Runtime?color=blue&style=flat-square)                                   |
| Lambdajection.Layer                    | ![Nuget](https://img.shields.io/nuget/v/Lambdajection.Layer?label=version&style=flat-square) ![Nuget](https://img.shields.io/nuget/dt/Lambdajection.Layer?color=blue&style=flat-square)                                       |
| Lambdajection.Framework                | ![Nuget](https://img.shields.io/nuget/v/Lambdajection.Framework?label=version&style=flat-square) ![Nuget](https://img.shields.io/nuget/dt/Lambdajection.Framework?color=blue&style=flat-square)                               |
| Lambdajection.Framework.BuildTime      | ![Nuget](https://img.shields.io/nuget/v/Lambdajection.Framework.BuildTime?label=version&style=flat-square) ![Nuget](https://img.shields.io/nuget/dt/Lambdajection.Framework.BuildTime?color=blue&style=flat-square)           |
| Lambdajection.CustomResource           | ![Nuget](https://img.shields.io/nuget/v/Lambdajection.CustomResource?label=version&style=flat-square) ![Nuget](https://img.shields.io/nuget/dt/Lambdajection.CustomResource?color=blue&style=flat-square)                     |
| Lambdajection.CustomResource.BuildTime | ![Nuget](https://img.shields.io/nuget/v/Lambdajection.CustomResource.BuildTime?label=version&style=flat-square) ![Nuget](https://img.shields.io/nuget/dt/Lambdajection.CustomResource.BuildTime?color=blue&style=flat-square) |

## 3. Templates

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
using System.Threading;
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

        public Task<object> Handle(object request, CancellationToken cancellationToken)
        {
            return new {
                foo = request
            };
        }
    }
}

```

### 4.2. Serialization

If your Lambda targets the `netcoreapp3.1` or `net5.0` frameworks, then by default, the serializer is set to  `DefaultLambdaJsonSerializer` from the `Amazon.Lambda.Serialization.SystemTextJson` package.  With any TFM, you may specify the serializer you want to use by setting the Lambda attribute's `Serializer` argument:

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

See the [full example here](./examples/CustomConfiguration/README.md).

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

### 4.7. Disposers

Disposers can be used to cleanup unmanaged resources, such as open file-handles and network connections. Lambdajection supports Lambdas that implement either [IDisposable](https://docs.microsoft.com/en-us/dotnet/api/system.idisposable), [IAsyncDisposable](https://docs.microsoft.com/en-us/dotnet/api/system.iasyncdisposable) or both.  If you implement both, DisposeAsync will be preferred.

### 4.8. Handler Scheme

When configuring your lambda on AWS, the method name you'll want to use will be `Run` (NOT `Handle`). For context, `Run` is a static method generated on your class during compilation.  It takes care of setting up the IoC container, if it hasn't been setup already.

So, going off the example above, the handler scheme would look like this:

```
Your.Assembly.Name::Your.Namespace.YourLambda::Run
```

You can customize the name of the "Run" method via the
RunnerMethod property of `LambdaAttribute`.

### 4.9. Custom Runtimes

Lambdajection can be used with custom runtimes starting in v0.5.0-beta2, that way you can use a newer version of .NET as soon as it comes out, even if it is not an LTS release.  

In order to use custom runtimes, add the `Lambdajection.Runtime` package to your csproj file. You must also specify the `RuntimeIdentifiers` property, with at least `linux-x64` included:

```xml
<Project>
    <PropertyGroup>
        <RuntimeIdentifiers>linux-x64</RuntimeIdentifiers>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="Lambdajection" Version="$(LambdajectionVersion)" />
        <PackageReference Include="Lambdajection.Runtime" Version="$(LambdajectionVersion)" />
    </ItemGroup>
</Project>
```

You may also optionally set the `SelfContained` property:
  - Set it to true if you want to deploy as a self-contained package.  In this case, Lambdajection will automatically set your assembly name to `bootstrap`.
  - Set it to false if you want to deploy as a framework dependent package, ie you installed .NET to a Lambda Layer and want to use that to cut-down on deployment package sizes. In this case, your assembly name will remain unchanged.  
  - In both cases, a main method / program entrypoint will be generated for you with the aid of `Amazon.Lambda.RuntimeSupport`.

See an example of a [non-self contained lambda using a custom runtime here](./examples/CustomRuntime). (Example documentation coming soon).  Note that the example is using [a Lambda Layer we deployed with a custom bootstrap file.](https://github.com/cythral/dotnet-lambda-layer)

### 4.10. Lambda Layer

You can use a Lambda Layer containing Lambdajection and all of its dependencies to cut down on package sizes. The layer will be available on the serverless application repository.  Once deployed, you can use it on functions that use the Lambdajection.Runtime package on custom runtimes containing .NET 5.

To use the layer:

1. Deploy lambdajection-layer from the [Serverless Application Repository](https://console.aws.amazon.com/lambda/home?region=us-east-1#/create/app?applicationId=arn:aws:serverlessrepo:us-east-1:918601311641:applications/lambdajection-layer) to your AWS Account.
   - You must use the same semantic version as the `Lambdajection` package in your project.
2. Note the value of the `LayerArn` output in the resulting stack and add it to your Lambda's list of layers.  See the [custom runtime example template](./examples/CustomRuntime/cloudformation.template.yml) on how to do this - specifically the CustomRuntime resource's Layers section.
3. Add the `Lambdajection.Runtime` and `Lambdajection.Layer` packages to your project (run `dotnet add package Lambdajection.Layer`).
   - Make sure to use the same version as the `Lambdajection` package in your project.
4. Finally, you will need to set the `DOTNET_SHARED_STORE` environment variable to `/opt/`.  This is because Lambda Layers are unzipped to that directory, and .NET needs to know where to look for the runtime package store.  
   - If using [dotnet-lambda-layer](https://github.com/cythral/dotnet-lambda-layer), this environment variable is set for you automatically.

## 5. Examples

- [Injecting and using AWS Services + Factories](examples/AwsClientFactories)
- [Automatic decryption of encrypted options](examples/EncryptedOptions)
- [Using a custom serializer](examples/CustomSerializer/README.md)
- [Using a custom config factory](examples/CustomConfiguration/README.md)
- [Using a custom runtime + layer](examples/CustomRuntime)

## 6. Acknowledgments

1. [CodeGeneration.Roslyn](https://github.com/aarnott/codegeneration.roslyn) - Was used for compile-time code generation using attributes in versions v0.1.0 - v0.4.0.  Newer versions use .NET 5 Source Generators.
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
