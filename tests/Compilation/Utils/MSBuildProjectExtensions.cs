using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Lambdajection.Generator;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using NSubstitute;

using static NSubstitute.Arg;

#pragma warning disable SA1009
namespace Microsoft.CodeAnalysis.MSBuild
{
    public static class MSBuildProjectExtensions
    {
        public static async Task<Project> LoadProject(string pathToProject)
        {
            using var workspace = MSBuildWorkspace.Create();
            workspace.WorkspaceFailed += (o, e) => Console.WriteLine(e.Diagnostic.Message);
            workspace.LoadMetadataForReferencedProjects = true;
            return await workspace.OpenProjectAsync(pathToProject);
        }

        public static async Task<ImmutableArray<Diagnostic>> GetGeneratorDiagnostics(this Project project)
        {
            var compilation = (await project.GetCompilationAsync())!;
            var generator = new Program();
            var optionsProvider = Substitute.For<AnalyzerConfigOptionsProvider>();
            var options = Substitute.For<AnalyzerConfigOptions>();
            optionsProvider.GlobalOptions.Returns(options);

            options.TryGetValue(Is("build_property.LambdajectionBuildTimeAssemblies"), out Any<string?>()).Returns(x =>
            {
                x[1] = TestMetadata.OutputPath + "/Lambdajection.Core.dll";
                return true;
            });

            options.TryGetValue(Is("build_property.LambdajectionAdditionalProbingPath"), out Any<string?>()).Returns(x =>
            {
                x[1] = TestMetadata.RestorePackagesPath;
                return true;
            });

            var driver = CSharpGeneratorDriver.Create(generators: new[] { generator }, optionsProvider: optionsProvider);
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var _, out var diagnostics);
            return diagnostics;
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
