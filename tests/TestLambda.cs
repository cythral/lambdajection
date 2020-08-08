using System;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lambdajection
{
    public class TestLambda : ILambda<object, object>
    {
        public object Request { get; set; } = null!;

        public ILambdaContext? Context { get; set; } = null;

        public virtual Task<object> Handle(object request, ILambdaContext? context)
        {
            Request = request;
            Context = context;

            return Task.FromResult<object>(null!);
        }
    }
}