using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Lambdajection.Generator;

using Microsoft.CodeAnalysis.CSharp;

#pragma warning disable SA1009
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
            var generator = new GenerationDriver();
            var driver = CSharpGeneratorDriver.Create(new[] { generator });
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var _, out var diagnotics);

            return diagnotics;
        }

        public static async Task<GenerateAssemblyResult> GenerateAssembly(this Project project)
        {
            var compilation = (await project.GetCompilationAsync())!;
            return compilation.GenerateAssembly();
        }

        public static Project WithoutReference(this Project project, string name)
        {
            var query = from reference in project.MetadataReferences
                        where reference.Display!.Contains(name, StringComparison.OrdinalIgnoreCase)
                        select reference;

            var referenceToRemove = query.First();
            return project.RemoveMetadataReference(referenceToRemove);
        }
    }
}
