using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Lambdajection.Examples.CustomSerializer
{
    public class EmbeddedResourceReader
    {
        public virtual async Task<string?> ReadAsString(string file)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using var stream = assembly.GetManifestResourceStream($"CustomSerializer.Resources.{file}");

                if (stream == null)
                {
                    return null;
                }

                using var reader = new StreamReader(stream);
                return await reader.ReadToEndAsync();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
