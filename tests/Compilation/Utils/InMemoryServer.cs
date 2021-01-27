using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Lambdajection
{
    public sealed class InMemoryServer : IDisposable
    {
        private readonly CancellationToken cancellationToken = new(false);
        private Task? handle;
        private HttpListener? listener;

        public InMemoryServer()
        {
            listener = new HttpListener();
            listener.Prefixes.Add(Address);
            listener.Start();
            handle = Run();
        }

        public string Address { get; } = GetUrl();

        public IList<Request> Requests { get; } = new List<Request>();

        public void Dispose()
        {
            var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            source.Cancel();

            if (listener != null)
            {
                listener.Abort();
                listener = null;
            }

            if (handle != null)
            {
                try
                {
                    handle.GetAwaiter().GetResult();
                }
                catch (Exception)
                {
                }

                handle.Dispose();
                handle = null;
            }

            GC.SuppressFinalize(this);
        }

        private static string GetUrl()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();

            return $"http://localhost:{port}/";
        }

        private Task Run()
        {
            return Task.Run(
                async () =>
                {
                    // GetContext method blocks while waiting for a request.
                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var context = await listener!.GetContextAsync();

                        cancellationToken.ThrowIfCancellationRequested();
                        Requests.Add(new Request
                        {
                            HttpMethod = context.Request.HttpMethod,
                            Body = await new StreamReader(context.Request.InputStream).ReadToEndAsync(),
                        });

                        var response = context.Response;
                        var stream = response.OutputStream;
                        var writer = new StreamWriter(stream);
                        writer.Write("OK");
                        writer.Close();
                    }
                },
                cancellationToken
            );
        }

        public class Request
        {
            public string HttpMethod { get; set; } = string.Empty;

            public string Body { get; set; } = string.Empty;
        }
    }
}
