using System.Diagnostics;
using System.IO;

using Microsoft.Build.Locator;

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
            MSBuildLocator.RegisterDefaults();
            Directory.SetCurrentDirectory(TestMetadata.TestDirectory);

            using var process = Process.Start("dotnet", $"restore Compilation/Projects/compilation-projects.sln -p:LambdajectionVersion={TestMetadata.PackageVersion}");
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                Exit(process.ExitCode);
            }
        }
    }
}
