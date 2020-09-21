using System.Collections.Generic;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using FluentAssertions;

using Lambdajection.Attributes;
using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace Lambdajection.Tests.Integration.ConfigFactory
{

    [Lambda(typeof(TestStartup), ConfigFactory = typeof(TestConfigFactory))]
    public partial class TestLambda
    {
        private readonly IConfiguration config;

        public TestLambda(IConfiguration config)
        {
            this.config = config;
        }

        public Task<string> Handle(string _, ILambdaContext context)
        {
            var value = config.GetValue<string>("TestKey");
            return Task.FromResult(value);
        }
    }

    public class TestStartup : ILambdaStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // do nothing
        }
    }

    public class TestConfigFactory : ILambdaConfigFactory
    {
        public IConfigurationRoot Create()
        {
            return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["TestKey"] = "TestValue"
            })
            .Build();
        }
    }


    [Category("Integration")]
    public class ConfigFactoryIntegrationTests
    {
        [Test]
        public async Task ConfigFactoryShouldBeUsed()
        {
            var result = await TestLambda.Run(null!, null!);
            result.Should().Be("TestValue");
        }
    }
}
