using System.Threading;
using System.Threading.Tasks;

using Lambdajection.CustomResource;

namespace Lambdajection
{
    public class TestCustomResourceLambda : ICustomResourceProvider<TestLambdaMessage, TestCustomResourceOutputData>
    {
        public virtual Task<TestCustomResourceOutputData> Create(CustomResourceRequest<TestLambdaMessage> request, CancellationToken cancellationToken)
        {
            return Task.FromResult((TestCustomResourceOutputData)null!);
        }

        public virtual Task<TestCustomResourceOutputData> Update(CustomResourceRequest<TestLambdaMessage> request, CancellationToken cancellationToken)
        {
            return Task.FromResult((TestCustomResourceOutputData)null!);
        }

        public virtual Task<TestCustomResourceOutputData> Delete(CustomResourceRequest<TestLambdaMessage> request, CancellationToken cancellationToken)
        {
            return Task.FromResult((TestCustomResourceOutputData)null!);
        }

        public virtual void Validate(CustomResourceRequest<TestLambdaMessage> request)
        {
        }

        public virtual bool RequiresReplacement(CustomResourceRequest<TestLambdaMessage> request)
        {
            return false;
        }
    }
}
