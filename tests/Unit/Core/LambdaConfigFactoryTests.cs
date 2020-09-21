using FluentAssertions;

using Microsoft.Extensions.Configuration.EnvironmentVariables;

using NUnit.Framework;

namespace Lambdajection.Core.Tests
{
    [Category("Unit")]
    public class LambdaConfigFactoryTests
    {
        [Test]
        public void CreateShouldAddEnvironmentVariables()
        {
            var factory = new LambdaConfigFactory();
            var configuration = factory.Create();
            configuration.Providers.Should().Contain(provider => provider.GetType() == typeof(EnvironmentVariablesConfigurationProvider));
        }
    }
}
