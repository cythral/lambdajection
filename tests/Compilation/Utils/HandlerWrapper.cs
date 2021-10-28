using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace Lambdajection
{
    public class HandlerWrapper<T>
    {
        private readonly Assembly assembly;
        private readonly string typeName;

        public HandlerWrapper(Assembly assembly, string typeName)
        {
            this.assembly = assembly;
            this.typeName = typeName;
        }

        public async Task<T?> Run(params object[] args)
        {
            var type = assembly.GetType(typeName)!;
            var runMethod = type.GetMethod("Run")!;

            var resultStream = await (Task<Stream>)runMethod.Invoke(null, args)!;
            return await JsonSerializer.DeserializeAsync<T>(resultStream);
        }
    }
}
