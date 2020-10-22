using System;
using System.Reflection;
using System.Runtime.Loader;

namespace Lambdajection
{
#pragma warning disable CA1063
    public class GenerateAssemblyResult : IDisposable
    {
        public Assembly Assembly { get; set; } = null!;

        public AssemblyLoadContext LoadContext { get; set; } = null!;

        public void Deconstruct(out Assembly assembly)
        {
            assembly = Assembly;
        }

        public void Deconstruct(out Assembly assembly, out AssemblyLoadContext context)
        {
            assembly = Assembly;
            context = LoadContext;
        }

        public void Dispose()
        {
            LoadContext.Unload();
            GC.SuppressFinalize(this);
        }
    }
}
