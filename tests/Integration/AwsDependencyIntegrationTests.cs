using System;
using System.Reflection;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;

using FluentAssertions;

using Lambdajection.Attributes;
using Lambdajection.Core;

using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NSubstitute;

using NUnit.Framework;

namespace Lambdajection.Tests.Integration.AwsDependency
{
    [Lambda(typeof(Startup))]
    public partial class ExampleLambda
    {
        private readonly ExampleBar exampleBar;
        private readonly ILogger<ExampleLambda> logger;
        private readonly IAmazonS3 s3Client;
        private readonly IAwsFactory<IAmazonS3> s3Factory;
        private readonly IAwsFactory<IAmazonSecurityTokenService> stsFactory;

        public ExampleLambda(ExampleBar exampleService, ILogger<ExampleLambda> logger, IAmazonS3 s3Client, IAwsFactory<IAmazonS3> s3Factory, IAwsFactory<IAmazonSecurityTokenService> stsFactory)
        {
            this.exampleBar = exampleService;
            this.logger = logger;
            this.s3Client = s3Client;
            this.s3Factory = s3Factory;
            this.stsFactory = stsFactory;
        }

        public Task<string> Handle(string request, ILambdaContext context)
        {
            logger.LogInformation("Test Logging Works");
            logger.LogInformation("S3 Client null: " + s3Client is null ? "true" : "false");
            logger.LogInformation("S3 Factory null: " + s3Factory is null ? "true" : "false");
            logger.LogInformation("STS Factory null: " + stsFactory is null ? "true" : "false");

            return Task.FromResult(request + " " + exampleBar.Bar());
        }
    }

    public class ExampleBar
    {
        private readonly string value;

        public ExampleBar()
        {
            value = "bar";
        }

        public string Bar()
        {
            return value;
        }
    }

    public class Startup : ILambdaStartup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection collection)
        {
            collection.AddScoped<ExampleBar>();
            collection.UseAwsService<IAmazonS3>();
            collection.UseAwsService<IAmazonS3>(); // intentionally added to ensure duplicate calls are handled ok
        }
    }

    [Category("Integration")]
    public class AwsDependencyIntegrationTests
    {
        [Test]
        public async Task TestExampleLambdaRun()
        {
            var test = "foo";
            var result = await ExampleLambda.Run(test, null!);

            result.Should().BeEquivalentTo("foo bar");
        }

        [Test]
        public async Task TestFactoriesDoNotPerformAssumeRoleIfNoRoleArnGiven()
        {
            var client = Substitute.For<IAmazonSecurityTokenService>();
            var configuratorType = typeof(ExampleLambda).GetNestedType("LambdajectionConfigurator", BindingFlags.NonPublic)!;
            var s3FactoryType = configuratorType.GetNestedType("S3Factory", BindingFlags.NonPublic)!;
            var factory = Activator.CreateInstance(s3FactoryType, new object[] { client });
            var createMethod = s3FactoryType.GetMethod("Create")!;

            createMethod.Invoke(factory, new object[] { null! });

            await client.DidNotReceive().AssumeRoleAsync(Arg.Any<AssumeRoleRequest>());
        }

        [Test]
        public async Task TestFactoriesPerformAssumeRoleIfRoleArnGiven()
        {
            var client = Substitute.For<IAmazonSecurityTokenService>();
            var configuratorType = typeof(ExampleLambda).GetNestedType("LambdajectionConfigurator", BindingFlags.NonPublic)!;
            var s3FactoryType = configuratorType.GetNestedType("S3Factory", BindingFlags.NonPublic)!;
            var factory = Activator.CreateInstance(s3FactoryType, new object[] { client });
            var createMethod = s3FactoryType.GetMethod("Create")!;
            var roleArn = "rolearn";

            createMethod.Invoke(factory, new object[] { roleArn });

            await client.Received().AssumeRoleAsync(Arg.Is<AssumeRoleRequest>(req => req.RoleArn == roleArn));
        }

        [Test]
        public async Task TestFactoryGenerationFailsIfRequestingAwsClientFactoryWithoutSecurityTokenAssembly()
        {
            MSBuildLocator.RegisterDefaults();

            using var workspace = MSBuildWorkspace.Create();
            workspace.LoadMetadataForReferencedProjects = true;

            // this path is relative to the output directory
            var project = await workspace.OpenProjectAsync("../../../../tests/Integration/NoFactoryCompilationTestProject/NoFactoryCompilationTestProject.csproj");
            var compilation = (await project.GetCompilationAsync())!;

            workspace.Diagnostics.Should().Contain(diagnostic => diagnostic.Message.Contains("[LJ0002] Add AWSSDK.SecurityToken as a dependency of your project to use AWS Factories."));
        }
    }
}
