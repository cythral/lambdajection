using System;
using System.Threading.Tasks;

namespace Lambdajection
{
#pragma warning disable CA1063,CA1816
    public class MultiDisposableLambda : TestLambda, IAsyncDisposable, IDisposable
    {
        public virtual async ValueTask DisposeAsync()
        {
            await Task.CompletedTask;
        }

        public virtual void Dispose()
        {
            // do nothing
        }
    }
}
