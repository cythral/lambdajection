using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using FluentAssertions;

using Lambdajection.Attributes;
using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NUnit.Framework;

namespace Lambdajection.Tests.Configuration
{
    [Lambda(Startup = typeof(TestStartup))]
    public partial class ConfigurationLambda
    {
        private ExampleOptions exampleOptions;

        public ConfigurationLambda(IOptions<ExampleOptions> exampleOptions)
        {
            this.exampleOptions = exampleOptions.Value;
        }

        public Task<string> Handle(string request, ILambdaContext context)
        {
            var test = typeof(OptionsConfigurator);
            return Task.FromResult(exampleOptions.ExampleConfigValue);
        }
    }

    [LambdaOptions(typeof(ConfigurationLambda), "Example")]
    public class ExampleOptions
    {
        public string ExampleConfigValue { get; set; } = "";
    }

    public class ConfigurationIntegrationTests
    {
        [Test]
        public async Task ConfigValueShouldBeReadFromEnvironment()
        {
            var configValue = "example config value";
            Environment.SetEnvironmentVariable("Example:ExampleConfigValue", configValue);

            var result = await ConfigurationLambda.Run("", null);
            result.Should().BeEquivalentTo(configValue);
        }
    }
}