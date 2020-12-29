using FluentAssertions;

using Microsoft.Extensions.Configuration.EnvironmentVariables;

using NUnit.Framework;

namespace Lambdajection.Core.Tests
{
    [Category("Unit")]
    public class LambdaConfigFactoryTests
    {
        [Test, Auto]
        public void CreateShouldAddEnvironmentVariables(
            [Target] LambdaConfigFactory factory
        )
        {
            var configuration = factory.Create();
            configuration.Providers.Should().Contain(provider => provider.GetType() == typeof(EnvironmentVariablesConfigurationProvider));
        }
    }
}
