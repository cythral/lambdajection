using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Lambdajection.Framework;

namespace Lambdajection.Examples.CustomSerializer
{
    public class EmbeddedResourceReader
    {
        private readonly List<ResourceContext> openContexts = new();

        public virtual async Task<string?> ReadAsString(string file)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var stream = assembly.GetManifestResourceStream($"CustomSerializer.Resources.{file}");

                if (stream == null)
                {
                    return null;
                }

                var reader = new StreamReader(stream);

                openContexts.Add(new ResourceContext
                {
                    Stream = stream,
                    Reader = reader,
                });

                return await reader.ReadToEndAsync();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public virtual async ValueTask DisposeOpenContexts()
        {
            foreach (var context in openContexts)
            {
                await context.DisposeAsync();
            }

            openContexts.Clear();
        }
    }
}
