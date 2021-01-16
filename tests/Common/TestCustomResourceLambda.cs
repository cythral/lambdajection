using System.Threading;
using System.Threading.Tasks;

using Lambdajection.CustomResource;

namespace Lambdajection
{
    public class TestCustomResourceLambda : ICustomResourceProvider<object, TestCustomResourceOutputData>
    {
        public virtual Task<TestCustomResourceOutputData> Create(CustomResourceRequest<object> request, CancellationToken cancellationToken)
        {
            return Task.FromResult((TestCustomResourceOutputData)null!);
        }

        public virtual Task<TestCustomResourceOutputData> Update(CustomResourceRequest<object> request, CancellationToken cancellationToken)
        {
            return Task.FromResult((TestCustomResourceOutputData)null!);
        }

        public virtual Task<TestCustomResourceOutputData> Delete(CustomResourceRequest<object> request, CancellationToken cancellationToken)
        {
            return Task.FromResult((TestCustomResourceOutputData)null!);
        }
    }
}
