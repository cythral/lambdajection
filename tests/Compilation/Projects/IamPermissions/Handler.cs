using System.Threading;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Lambdajection.Attributes;
using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lambdajection.CompilationTests.IamPermissions
{
    [Lambda(typeof(Startup))]
    public partial class Handler
    {
        private readonly Utility utility;

        public Handler(Utility utility)
        {
            this.utility = utility;
        }

        public async Task<string> Handle(string request, CancellationToken cancellationToken = default)
        {
            utility.ArbitraryOperation();
            await utility.GetObject();
            return (string)null!;
        }
    }
}

