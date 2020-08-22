using System;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.S3;

using FluentAssertions;

using Lambdajection.Attributes;
using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NUnit.Framework;

namespace Lambdajection.Tests
{
    [Lambda(Startup = typeof(Startup))]
    public partial class ExampleLambda
    {
        private readonly ExampleBar exampleBar;
        private readonly ILogger<ExampleLambda> logger;
        private readonly IAmazonS3 s3Client;

        public ExampleLambda(ExampleBar exampleService, ILogger<ExampleLambda> logger, IAmazonS3 s3Client)
        {
            this.exampleBar = exampleService;
            this.logger = logger;
            this.s3Client = s3Client;
        }

        public Task<string> Handle(string request, ILambdaContext context)
        {
            logger.LogInformation("Test Logging Works");
            logger.LogInformation("S3 Client null: " + s3Client is null ? "true" : "false");

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
        public IConfiguration Configuration { get; set; } = default!;

        public void ConfigureServices(IServiceCollection collection)
        {
            collection.AddScoped<ExampleBar>();
            collection.UseAwsService<IAmazonS3>();
            collection.UseAwsService<IAmazonS3>(); // intentionally added to ensure duplicate calls are handled ok
        }
    }

    public class IntegrationTests
    {
        [Test]
        public async Task TestExampleLambdaRun()
        {
            Environment.SetEnvironmentVariable("AWS_REGION", "us-east-1");
            Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", "key-id");
            Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", "secret-key");

            var test = "foo";
            var result = await ExampleLambda.Run(test, null);

            result.Should().BeEquivalentTo("foo bar");
        }
    }
}
