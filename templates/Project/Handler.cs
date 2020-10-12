using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Lambdajection.Attributes;

namespace __Project__
{
    [Lambda(typeof(Startup))]
    public partial class Handler
    {
        public async Task<object> Handle(object request, ILambdaContext context)
        {
            return await Task.FromResult(new { });
        }
    }
}
