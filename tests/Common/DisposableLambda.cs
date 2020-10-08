using System;

namespace Lambdajection
{
#pragma warning disable CA1063,CA1816
    public class DisposableLambda : TestLambda, IDisposable
    {
        public virtual void Dispose()
        {
            // do nothing
        }
    }
}
