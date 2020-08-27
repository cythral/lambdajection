using System;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using FluentAssertions;

using Lambdajection.Attributes;
using Lambdajection.Core;
using Lambdajection.Encryption;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NSubstitute;

using NUnit.Framework;

namespace Lambdajection.Tests.Configuration
{

    [Lambda(Startup = typeof(Startup))]
    public partial class ConfigurationLambda
    {
        private readonly ExampleOptions exampleOptions;

        public ConfigurationLambda(IOptions<ExampleOptions> exampleOptions)
        {
            this.exampleOptions = exampleOptions.Value;
        }

        public Task<ExampleOptions> Handle(string request, ILambdaContext context)
        {
            return Task.FromResult(exampleOptions);
        }
    }

    public class Startup : ILambdaStartup
    {
        public static IDecryptionService DecryptionService { get; } = Substitute.For<IDecryptionService>();
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            DecryptionService.Decrypt(Arg.Any<string>()).Returns(x => "[decrypted] " + x.ArgAt<string>(0));
            services.AddSingleton(DecryptionService);
        }
    }

    [LambdaOptions(typeof(ConfigurationLambda), "Example")]
    public class ExampleOptions
    {
        public string ExampleConfigValue { get; set; } = "";

        [Encrypted]
        public string ExampleEncryptedValue { get; set; } = "";
    }

    [Category("Integration")]
    public class ConfigurationIntegrationTests
    {
        private const string exampleConfigValue = "example config value";
        private const string exampleEncryptedValue = "example encrypted config value";

        [SetUp]
        public void SetupEnvironmentVariables()
        {
            Environment.SetEnvironmentVariable("Example:ExampleConfigValue", exampleConfigValue);
            Environment.SetEnvironmentVariable("Example:ExampleEncryptedValue", exampleEncryptedValue);
        }


        [Test]
        public async Task ConfigValueShouldBeReadFromEnvironment()
        {
            var result = await ConfigurationLambda.Run("", null);
            result.ExampleConfigValue.Should().BeEquivalentTo(exampleConfigValue);
        }

        [Test]
        public async Task EncryptedConfigValueShouldBeReadFromEnvironmentAndDecrypted()
        {
            var result = await ConfigurationLambda.Run("", null);
            result.ExampleEncryptedValue.Should().BeEquivalentTo("[decrypted] " + exampleEncryptedValue);
        }

    }
}
