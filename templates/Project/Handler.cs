using System.Threading;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using Lambdajection.Attributes;

namespace __Project__
{
    [Lambda(typeof(Startup))]
#if disposable
    public partial class Handler : IAsyncDisposable, IDisposable
#else
    public partial class Handler
#endif
    {
#if disposable
        private bool disposed;

#endif
        public async Task<object> Handle(object request, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(new { });
        }
#if disposable

        #region Disposable Methods
        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            await Task.CompletedTask;
            // Perform asynchronous cleanup here
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }
            
            if (disposing)
            {
                // Perform synchronous cleanup here
            }

            disposed = true;
        }
        #endregion
#endif
    }
}
