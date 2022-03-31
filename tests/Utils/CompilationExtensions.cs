using System;
using System.IO;
using System.Linq;
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

        foreach (var reference in compilation.References)
        {
            var display = reference.Display!;

            try
            {
                if (!AppDomain.CurrentDomain.GetAssemblies().Any(a => a.Location == display))
                {
                    context.LoadFromAssemblyPath(display);
                }
            }
            catch (NotSupportedException)
            {
            }
            catch (FileLoadException)
            {
            }
            catch (BadImageFormatException)
            {
            }
        }

        return new GenerateAssemblyResult
        {
            Assembly = context.LoadFromStream(stream),
            LoadContext = context,
        };
    }
}
