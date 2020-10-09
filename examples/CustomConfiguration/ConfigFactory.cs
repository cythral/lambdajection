using System.IO;

using Lambdajection.Core;

using Microsoft.Extensions.Configuration;

namespace Lambdajection.Examples.CustomConfiguration
{
    public class ConfigFactory : ILambdaConfigFactory
    {
        public IConfigurationRoot Create()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
        }
    }
}
