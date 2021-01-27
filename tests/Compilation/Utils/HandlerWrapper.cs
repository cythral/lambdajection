using System.Reflection;
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

        public async Task<T> Run(params object[] args)
        {
            var type = assembly.GetType(typeName)!;
            var runMethod = type.GetMethod("Run")!;

            var task = (Task)runMethod.Invoke(null, args)!;
            await task;

            return (T)((dynamic)task).Result;
        }
    }
}
