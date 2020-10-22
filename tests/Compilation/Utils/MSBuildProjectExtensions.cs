using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;

using Lambdajection;
using Lambdajection.Generator;

using Microsoft.CodeAnalysis.CSharp;

namespace Microsoft.CodeAnalysis.MSBuild
{
    public static class MSBuildProjectExtensions
    {
        public static async Task<Project> LoadProject(string pathToProject)
        {
            using var workspace = MSBuildWorkspace.Create();
            workspace.LoadMetadataForReferencedProjects = true;

            return await workspace.OpenProjectAsync(pathToProject);
        }

        public static async Task<ImmutableArray<Diagnostic>> GetGeneratorDiagnostics(this Project project)
        {
            var compilation = (await project.GetCompilationAsync())!;
            var generator = new LambdaGenerator();
            var driver = CSharpGeneratorDriver.Create(new[] { generator });

            driver.RunGeneratorsAndUpdateCompilation(compilation, out var _, out var diagnotics);

            return diagnotics;
        }

        public static async Task<GenerateAssemblyResult> GenerateAssembly(this Project project)
        {
            var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithPlatform(Platform.X64)
                .WithOptimizationLevel(OptimizationLevel.Release);

            var compilation = (await project.GetCompilationAsync())!;
            var generator = new LambdaGenerator();
            var driver = CSharpGeneratorDriver.Create(new[] { generator });

            driver.RunGeneratorsAndUpdateCompilation(compilation, out var result, out var _);

            var diagnostics = result.GetDiagnostics();
            if (diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Any())
            {
                throw new Exception("Compilation failed to generate\n" + string.Join('\n', diagnostics.Select(diagnostic => diagnostic.GetMessage())));
            }

            using var stream = new MemoryStream();
            result.WithOptions(options).Emit(stream);
            stream.Position = 0;

            var context = new AssemblyLoadContext(Path.GetRandomFileName(), true);
            var tempContext = new AssemblyLoadContext(Path.GetRandomFileName(), true);

            foreach (var reference in result.References)
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
                catch (BadImageFormatException) { }
            }

            tempContext.Unload();

            return new GenerateAssemblyResult
            {
                Assembly = context.LoadFromStream(stream),
                LoadContext = context,
            };
        }

        public static Project WithoutReference(this Project project, string name)
        {
            var query = from reference in project.MetadataReferences
                        where reference.Display!.Contains(name, System.StringComparison.OrdinalIgnoreCase)
                        select reference;

            var referenceToRemove = query.First();
            return project.RemoveMetadataReference(referenceToRemove);
        }
    }
}
