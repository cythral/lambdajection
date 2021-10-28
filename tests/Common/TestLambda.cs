using System.Threading;
using System.Threading.Tasks;

using Lambdajection.Core;

namespace Lambdajection
{
    public class TestLambda : ILambda<TestLambdaMessage, TestLambdaMessage>
    {
        public object Request { get; set; } = null!;

        public virtual Task<TestLambdaMessage> Handle(TestLambdaMessage request, CancellationToken cancellationToken = default)
        {
            Request = request;

            return Task.FromResult<TestLambdaMessage>(null!);
        }

        public void Validate(TestLambdaMessage request)
        {
        }
    }
}
