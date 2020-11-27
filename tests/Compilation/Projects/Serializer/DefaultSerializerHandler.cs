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
    [Lambda(typeof(Startup))]
    public partial class DefaultSerializerHandler
    {
        public Task<string> Handle(string request)
        {
            return Task.FromResult("");
        }
    }
}
