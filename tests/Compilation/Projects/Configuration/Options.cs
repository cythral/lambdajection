using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Lambdajection.Attributes;
using Lambdajection.Core;
using Lambdajection.Encryption;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lambdajection.CompilationTests.Configuration
{

    [LambdaOptions(typeof(Handler), "Example")]
    public class Options
    {
        public string ExampleConfigValue { get; set; } = "";

        [Encrypted]
        public string ExampleEncryptedValue { get; set; } = "";
    }
}