using System.IO;
using System.Threading.Tasks;

using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;

using FluentAssertions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

using NUnit.Framework;

#pragma warning disable SA1009
namespace Lambdajection.Tests.Compilation
{
    [Category("Integration")]
    public class IamPermissionsTests
    {
        private const string ProjectPath = "Compilation/Projects/IamPermissions/IamPermissions.csproj";

        private static readonly string IamDocPath = $"{TestMetadata.BaseIntermediateOutputPath}/CompilationTestProjects/IamPermissions/Debug/{TestMetadata.TargetFramework}/Handler.iam.txt";

        private static Project project = null!;

        [OneTimeSetUp]
        public async Task Setup()
        {
            project = await MSBuildProjectExtensions.LoadProject(ProjectPath);
        }

        [Test, Auto]
        public async Task Permissions_ShouldContainArbitraryOperation1(
            string roleArn,
            AssumeRoleResponse response,
            Credentials credentials,
            IAmazonSecurityTokenService stsClient
        )
        {
            using var generation = await project.GenerateAssembly();
            var iamDocText = await File.ReadAllTextAsync(IamDocPath);
            var permissions = iamDocText.ToString().Split('\n');
            permissions.Should().Contain("ec2:ArbitraryOperation1");
        }

        [Test, Auto]
        public async Task Permissions_ShouldContainArbitraryOperation2(
            string roleArn,
            AssumeRoleResponse response,
            Credentials credentials,
            IAmazonSecurityTokenService stsClient
        )
        {
            using var generation = await project.GenerateAssembly();
            var iamDocText = await File.ReadAllTextAsync(IamDocPath);
            var permissions = iamDocText.ToString().Split('\n');
            permissions.Should().Contain("ec2:ArbitraryOperation2");
        }

        [Test, Auto]
        public async Task Permissions_ShouldContainGetObject(
            string roleArn,
            AssumeRoleResponse response,
            Credentials credentials,
            IAmazonSecurityTokenService stsClient
        )
        {
            using var generation = await project.GenerateAssembly();
            var iamDocText = await File.ReadAllTextAsync(IamDocPath);
            var permissions = iamDocText.ToString().Split('\n');
            permissions.Should().Contain("s3:GetObject");
        }
    }
}
