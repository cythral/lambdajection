using System.Threading;
using System.Threading.Tasks;

using Lambdajection.Sns;

namespace Lambdajection
{
    public class TestSnsLambda : ISnsEventHandler<TestLambdaMessage, TestLambdaMessage>
    {
        public object Request { get; set; } = null!;

        public virtual Task<TestLambdaMessage> Handle(SnsMessage<TestLambdaMessage> request, CancellationToken cancellationToken = default)
        {
            Request = request;

            return Task.FromResult<TestLambdaMessage>(null!);
        }

        public virtual void Validate(SnsMessage<TestLambdaMessage> request)
        {
        }
    }
}
