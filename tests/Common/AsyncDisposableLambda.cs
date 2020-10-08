using System;
using System.Threading.Tasks;

namespace Lambdajection
{
#pragma warning disable CA1063,CA1816
    public class AsyncDisposableLambda : TestLambda, IAsyncDisposable
    {
        public virtual async ValueTask DisposeAsync()
        {
            await Task.CompletedTask;
        }
    }
}
