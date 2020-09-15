using Lambdajection.Core;

using Microsoft.Extensions.Configuration;

namespace Lambdajection
{
    public class TestConfigFactory : ILambdaConfigFactory
    {
#pragma warning disable CA1822
        public IConfigurationRoot Create()
        {
            return new ConfigurationBuilder().Build();
        }
#pragma warning restore CA1822
    }
}