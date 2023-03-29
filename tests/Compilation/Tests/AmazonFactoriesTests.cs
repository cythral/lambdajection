using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Amazon.Runtime;
using Amazon.S3;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;

using FluentAssertions;

using Lambdajection.Core;
using Lambdajection.Utils;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

using NSubstitute;

using NUnit.Framework;

using YamlDotNet.Serialization;

using static NSubstitute.Arg;

#pragma warning disable SA1009
namespace Lambdajection.Tests.Compilation
{
    [Category("Integration")]
    public class AmazonFactoriesTests
    {
        private const string ProjectPath = "Compilation/Projects/AmazonFactories/AmazonFactories.csproj";
        private static readonly string TemplatePath = $"{TestMetadata.BaseOutputPath}/CompilationTestProjects/AmazonFactories/Debug/{TestMetadata.TargetFramework}/Handler.template.yml";

        private static Project project = null!;

        [OneTimeSetUp]
        public async Task Setup()
        {
            Console.WriteLine("test");
            project = await MSBuildProjectExtensions.LoadProject(ProjectPath);
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
        public async Task EmittedFactories_ThrowsIfCancellationRequested(
            IAmazonSecurityTokenService stsClient
        )
        {
            using var generation = await project.GenerateAssembly();
            var (assembly, _) = generation;
            var factoryType = assembly.GetType("Lambdajection.CompilationTests.AmazonFactories.Handler+LambdajectionConfigurator+S3Factory");
            var factory = (IAwsFactory<IAmazonS3>)Activator.CreateInstance(factoryType!, new object[] { stsClient })!;

            var cancellationToken = new CancellationToken(true);
            Func<Task> func = async () => await factory.Create(cancellationToken: cancellationToken);

            await func.Should().ThrowAsync<OperationCanceledException>();
            await stsClient.DidNotReceive().AssumeRoleAsync(Any<AssumeRoleRequest>());
        }

        [Test, Auto]
        public async Task Handler_RunsWithoutError()
        {
            using var generation = await project.GenerateAssembly();
            var (assembly, _) = generation;
            var handlerType = assembly.GetType("Lambdajection.CompilationTests.AmazonFactories.Handler")!;
            var runMethod = handlerType.GetMethod("Run")!;

            using var inputStream = await StreamUtils.CreateJsonStream("foo");
            var resultStream = await (Task<Stream>)runMethod.Invoke(null, new[] { inputStream, null })!;
            var result = await JsonSerializer.DeserializeAsync<string>(resultStream);

            result.Should().Be("ok");
        }

        [Test, Auto]
        public async Task GeneratedTemplate_ShouldHaveHandlerLambda(
            string roleArn,
            AssumeRoleResponse response,
            Credentials credentials,
            IAmazonSecurityTokenService stsClient
        )
        {
            var deserializer = new DeserializerBuilder()
            .WithNodeDeserializer(new IntrinsicNodeDeserializer())
            .WithNodeTypeResolver(new IntrinsicNodeTypeResolver())
            .Build();

            using var generation = await project.GenerateAssembly();
            var templateText = await File.ReadAllTextAsync(TemplatePath);
            var template = deserializer.Deserialize<dynamic>(templateText.ToString());
            ((string)template["Resources"]["HandlerLambda"]["Type"]).Should().Be("AWS::Lambda::Function");
        }

        [Test, Auto]
        public async Task GeneratedTemplate_ShouldHaveHandlerRole(
            string roleArn,
            AssumeRoleResponse response,
            Credentials credentials,
            IAmazonSecurityTokenService stsClient
        )
        {
            var deserializer = new DeserializerBuilder()
            .WithNodeDeserializer(new IntrinsicNodeDeserializer())
            .WithNodeTypeResolver(new IntrinsicNodeTypeResolver())
            .Build();

            using var generation = await project.GenerateAssembly();
            var templateText = await File.ReadAllTextAsync(TemplatePath);
            var template = deserializer.Deserialize<dynamic>(templateText.ToString());
            ((string)template["Resources"]["HandlerRole"]["Type"]).Should().Be("AWS::IAM::Role");
        }
    }
}
