using System;
using System.Threading;
using System.Threading.Tasks;

using Lambdajection.Core;

namespace Lambdajection
{
    public class TestLambda : ILambda<TestLambdaMessage, TestLambdaMessage>
    {
        public object Request { get; set; } = null!;

        public Action? HandleAction { get; set; } = null;

        public virtual Task<TestLambdaMessage> Handle(TestLambdaMessage request, CancellationToken cancellationToken = default)
        {
            Request = request;
            HandleAction?.Invoke();
            return Task.FromResult<TestLambdaMessage>(null!);
        }

        public virtual void Validate(TestLambdaMessage request)
        {
        }
    }
}
