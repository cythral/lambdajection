using System.Threading;
using System.Threading.Tasks;

using Lambdajection.Core;

namespace Lambdajection
{
    public class TestLambda : ILambda<object, object>
    {
        public object Request { get; set; } = null!;

        public virtual Task<object> Handle(object request, CancellationToken cancellationToken = default)
        {
            Request = request;

            return Task.FromResult<object>(null!);
        }

        public void Validate(object request)
        {
        }
    }
}
