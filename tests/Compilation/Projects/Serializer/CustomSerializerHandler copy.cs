using System;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Lambdajection.Attributes;
using Lambdajection.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Lambdajection.CompilationTests.Serializer
{
    [Lambda(typeof(Startup), Serializer = typeof(TestSerializer))]
    public partial class CustomSerializerHandler
    {
        public Task<string> Handle(string request, ILambdaContext context)
        {
            return Task.FromResult("");
        }
    }
}