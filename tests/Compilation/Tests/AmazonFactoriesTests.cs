
using System;
using System.Reflection;
using System.Threading.Tasks;

using Amazon.Runtime;
using Amazon.S3;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;

using FluentAssertions;

using Lambdajection.Core;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

namespace Lambdajection.Tests.Compilation
{
    [Category("Integration")]
    public class AmazonFactoriesTests
    {
        private const string projectPath = "Compilation/Projects/AmazonFactories/AmazonFactories.csproj";

        private static Project project = null!;

        [OneTimeSetUp]
        public async Task Setup()
        {
            project = await MSBuildProjectExtensions.LoadProject(projectPath);
        }

        [Test]
        public async Task ProjectWithSTSReference_DoesNotFailWithLJ0002()
        {
            var diagnostics = await project.GetGeneratorDiagnostics();
            diagnostics.Should().NotContain(diagnostic => diagnostic.Id == "LJ0002");
        }

        [Test]
        public async Task ProjectWithoutSTSReference_FailsWithLJ0002()
        {
            var projectWithoutSts = project.WithoutReference("AWSSDK.SecurityToken");

            var diagnostics = await projectWithoutSts.GetGeneratorDiagnostics();
            diagnostics.Should().Contain(diagnostic => diagnostic.Id == "LJ0002");
        }

        [Test, Auto]
        public async Task EmittedFactories_PerformAssumeRole_IfArnGiven(
            string roleArn,
            AssumeRoleResponse response,
            Credentials credentials,
            IAmazonSecurityTokenService stsClient
        )
        {
            response.Credentials = credentials;
            stsClient.AssumeRoleAsync(Any<AssumeRoleRequest>()).Returns(response);

            using var generation = await project.GenerateAssembly();
            var (assembly, _) = generation;
            var factoryType = assembly.GetType("Lambdajection.CompilationTests.AmazonFactories.Handler+LambdajectionConfigurator+S3Factory");
            var factory = (IAwsFactory<IAmazonS3>)Activator.CreateInstance(factoryType!, new object[] { stsClient })!;
            var result = await factory.Create(roleArn);

            var credentialsProperty = typeof(AmazonServiceClient).GetProperty("Credentials", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var actualCredentials = credentialsProperty.GetMethod!.Invoke(result, Array.Empty<object>());

            actualCredentials.Should().BeSameAs(credentials);
            await stsClient.Received().AssumeRoleAsync(Is<AssumeRoleRequest>(req =>
                req.RoleArn == roleArn
            ));
        }

        [Test, Auto]
        public async Task EmittedFactories_DoNotPerformAssumeRole_IfNoArnGiven(
            IAmazonSecurityTokenService stsClient
        )
        {
            using var generation = await project.GenerateAssembly();
            var (assembly, _) = generation;
            var factoryType = assembly.GetType("Lambdajection.CompilationTests.AmazonFactories.Handler+LambdajectionConfigurator+S3Factory");
            var factory = (IAwsFactory<IAmazonS3>)Activator.CreateInstance(factoryType!, new object[] { stsClient })!;

            var result = await factory.Create();

            result.Should().NotBeNull();
            await stsClient.DidNotReceive().AssumeRoleAsync(Any<AssumeRoleRequest>());
        }

        [Test, Auto]
        public async Task Handler_RunsWithoutError()
        {
            using var generation = await project.GenerateAssembly();
            var (assembly, _) = generation;
            var handlerType = assembly.GetType("Lambdajection.CompilationTests.AmazonFactories.Handler")!;
            var runMethod = handlerType.GetMethod("Run")!;

            var result = await (Task<IAwsFactory<IAmazonS3>>)runMethod.Invoke(null, new[] { "foo", null })!;

            result.Should().NotBeNull();
        }
    }
}
