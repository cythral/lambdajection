using System.Diagnostics;
using System.IO;

using NUnit.Framework;

using static System.Environment;

namespace Lambdajection.Tests.Compilation
{
    [SetUpFixture]
    [Category("Integration")]
    public class Setup
    {
        [OneTimeSetUp]
        public void RestoreCompilationProjects()
        {
            Directory.SetCurrentDirectory(TestMetadata.TestDirectory);

            using var restoreProcess = Process.Start("dotnet", $"restore Compilation/Projects/compilation-projects.sln -p:LambdajectionVersion={TestMetadata.PackageVersion} -t:Restore,RestoreGeneratorDependencies");
            restoreProcess.WaitForExit();

            if (restoreProcess.ExitCode != 0)
            {
                Exit(restoreProcess.ExitCode);
            }
        }
    }
}
