using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Lambdajection.Attributes;
using Lambdajection.Core;
using Lambdajection.Framework;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lambdajection.CompilationTests.IamPermissions
{
    public class Utility
    {
        public Utility()
        {
        }

        [RequiresIamPermission("ec2:ArbitraryOperation1")]
        [RequiresIamPermission("ec2:ArbitraryOperation2")]
        public void AbitraryOperation()
        {
        }
    }
}
