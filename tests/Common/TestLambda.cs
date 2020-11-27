using System.Threading.Tasks;

using Lambdajection.Core;

namespace Lambdajection
{
    public class TestLambda : ILambda<object, object>
    {
        public object Request { get; set; } = null!;

        public virtual Task<object> Handle(object request)
        {
            Request = request;

            return Task.FromResult<object>(null!);
        }
    }
}
