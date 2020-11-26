using System;
using System.IO;
using System.Threading.Tasks;

namespace Lambdajection.Examples.CustomSerializer
{
    public class ResourceContext : IAsyncDisposable
    {
        private bool disposed = false;

        public Stream? Stream { get; set; }

        public StreamReader? Reader { get; set; }

        #region Disposable Methods

        public async ValueTask DisposeAsync()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            await DisposeStream();
            DisposeReader();
            GC.SuppressFinalize(this);
        }

        private void DisposeReader()
        {
            Reader?.Dispose();
            Reader = null;
        }

        private async ValueTask DisposeStream()
        {
            if (Stream is Stream notNullStream)
            {
                await notNullStream.DisposeAsync();
                Stream = null;
            }
        }

        #endregion
    }
}
