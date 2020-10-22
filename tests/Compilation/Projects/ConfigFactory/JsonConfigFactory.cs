using System.IO;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Lambdajection.Attributes;
using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lambdajection.CompilationTests.ConfigFactory
{
    public class JsonConfigFactory : ILambdaConfigFactory
    {
        public IConfigurationRoot Create()
        {
            return new ConfigurationBuilder()
            // when this is loaded into the test assembly, the CWD will be the tests folder
            .SetBasePath(Directory.GetCurrentDirectory() + "/Compilation/Projects/ConfigFactory/")
            .AddJsonFile("appsettings.json", optional: true)
            .Build();
        }
    }
}