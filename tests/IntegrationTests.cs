using System.Threading.Tasks;

using Amazon.Lambda.Core;

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
        private ExampleBar exampleBar;
        private ILogger<ExampleLambda> logger;

        public ExampleLambda(ExampleBar exampleService, ILogger<ExampleLambda> logger)
        {
            this.exampleBar = exampleService;
            this.logger = logger;
        }

        public Task<string> Handle(string request, ILambdaContext context)
        {
            logger.LogInformation("Test Logging Works");

            return Task.FromResult(request + " " + exampleBar.Bar());
        }
    }

    public class ExampleBar
    {
        private string value;

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
        }
    }

    public class IntegrationTests
    {
        [Test]
        public async Task TestExampleLambdaRun()
        {
            var test = "foo";
            var result = await ExampleLambda.Run(test, null);

            result.Should().BeEquivalentTo("foo bar");
        }
    }
}