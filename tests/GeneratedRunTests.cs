using System.Threading.Tasks;

using Amazon.Lambda.Core;

using FluentAssertions;

using Lambdajection.Attributes;
using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace Lambdajection
{
    [Lambda(Startup = typeof(Startup))]
    public partial class ExampleLambda
    {
        private ExampleBar exampleBar;

        public ExampleLambda(ExampleBar exampleService)
        {
            this.exampleBar = exampleService;
        }

        public Task<string> Handle(string request, ILambdaContext context)
        {
            return Task.FromResult(request + " " + exampleBar.Bar());
        }
    }

    public class ExampleBar
    {
        public string Bar()
        {
            return "Bar";
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

    public class GeneratedRunTests
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