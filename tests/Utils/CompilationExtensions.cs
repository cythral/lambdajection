using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public static class CompilationExtensions
{
    public static GenerateAssemblyResult GenerateAssembly(this Compilation compilation)
    {
        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithPlatform(Platform.AnyCpu)
            .WithOptimizationLevel(OptimizationLevel.Release);

        var diagnostics = compilation.GetDiagnostics();
        if (diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Any())
        {
            throw new Exception("Compilation failed to generate\n" + string.Join('\n', diagnostics.Select(diagnostic => diagnostic.GetMessage())));
        }

        using var stream = new MemoryStream();
        compilation.WithOptions(options).Emit(stream);
        stream.Position = 0;

        var context = new AssemblyLoadContext(Path.GetRandomFileName(), true);
        var tempContext = new AssemblyLoadContext(Path.GetRandomFileName(), true);

        foreach (var reference in compilation.References)
        {
            var display = reference.Display!;

            try
            {
                var assembly = tempContext.LoadFromAssemblyPath(display);
                var thisAssembly = Assembly.GetExecutingAssembly();

                if (!thisAssembly.GetReferencedAssemblies().Any(a => a.FullName == assembly.FullName))
                {
                    context.LoadFromAssemblyPath(display);
                }
            }
            catch (FileLoadException)
            {
            }
            catch (BadImageFormatException)
            {
            }
        }

        tempContext.Unload();

        return new GenerateAssemblyResult
        {
            Assembly = context.LoadFromStream(stream),
            LoadContext = context,
        };
    }
}
