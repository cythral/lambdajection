using System;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

using AutoFixture.AutoNSubstitute;

using FluentAssertions;

using Lambdajection.CustomResource;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

using NSubstitute;

using NUnit.Framework;

#pragma warning disable SA1009, SA1402, SA1649
namespace Lambdajection.Tests.Compilation
{
    public class ResponseData : ICustomResourceOutputData
    {
        public string Id { get; set; } = string.Empty;

        public string MethodCalled { get; set; } = string.Empty;
    }

    [Category("Integration")]
    public class CustomResourcesTests
    {
        private const string projectPath = "Compilation/Projects/CustomResources/CustomResources.csproj";
        private const string handlerTypeName = "Lambdajection.CompilationTests.CustomResources.Handler";
        private const string resourcePropertiesTypeName = "Lambdajection.CompilationTests.CustomResources.ResourceProperties";
        private const string metadataTypeName = "Lambdajection.CompilationTests.CustomResources.Metadata";

        private static Project project = null!;

        public static dynamic CreateRequest(Assembly assembly, string name, bool shouldFail = false, string? errorMessage = null)
        {
            var resourcePropertiesType = assembly.GetType(resourcePropertiesTypeName)!;
            var metadataType = assembly.GetType(metadataTypeName)!;
            dynamic resourceProperties = Activator.CreateInstance(resourcePropertiesType)!;
            dynamic oldResourceProperties = Activator.CreateInstance(resourcePropertiesType)!;
            dynamic metadata = Activator.CreateInstance(metadataType)!;
            dynamic oldMetadata = Activator.CreateInstance(metadataType)!;

            resourceProperties.Name = oldResourceProperties.Name = name;
            resourceProperties.ShouldFail = oldResourceProperties.ShouldFail = shouldFail;
            resourceProperties.ErrorMessage = oldResourceProperties.ErrorMessage = errorMessage;

            var requestType = typeof(CustomResourceRequest<>).MakeGenericType(resourcePropertiesType);
            dynamic request = Activator.CreateInstance(requestType)!;
            request.ResourceProperties = resourceProperties;
            request.ResourceProperties.Metadata = metadata;
            request.OldResourceProperties = oldResourceProperties;
            request.OldResourceProperties.Metadata = oldMetadata;

            return request;
        }

        [OneTimeSetUp]
        public async Task Setup()
        {
            project = await MSBuildProjectExtensions.LoadProject(projectPath);
        }

        [Test, Auto]
        public async Task Run_PerformsUpdates(
            string name,
            string requestId,
            string stackId,
            string logicalResourceId,
            [Substitute] ILambdaContext context
        )
        {
            using var generation = await project.GenerateAssembly();
            using var server = new InMemoryServer();

            var requestType = CustomResourceRequestType.Update;
            var (assembly, _) = generation;
            var handler = new HandlerWrapper<object>(assembly, handlerTypeName);
            var request = CreateRequest(assembly, name);
            request.RequestType = requestType;
            request.ResponseURL = new Uri(server.Address);
            request.StackId = stackId;
            request.RequestId = requestId;
            request.LogicalResourceId = logicalResourceId;

            using var inputStream = await StreamUtils.CreateJsonStream(request);
            await handler.Run(inputStream, context);

            var httpRequest = server.Requests
            .Should()
            .Contain(request =>
                request.HttpMethod == "PUT"
            )
            .Which;

            // Tests to make sure status is serialized to all caps
            httpRequest.Body.Should().MatchRegex("\"Status\":[ ]?\"SUCCESS\"");

            var body = JsonSerializer.Deserialize<CustomResourceResponse<ResponseData>>(httpRequest.Body);
            body.Should().Match<CustomResourceResponse<ResponseData>>(response =>
                response.PhysicalResourceId == name &&
                response.StackId == stackId &&
                response.RequestId == requestId &&
                response.LogicalResourceId == logicalResourceId &&
                response.Status == CustomResourceResponseStatus.Success &&
                response.Data!.MethodCalled == requestType.ToString()
            );
        }

        [Test, Auto]
        public async Task Run_PerformsCreateIfUpdateRequiresReplacement(
            string name,
            string requestId,
            string stackId,
            string logicalResourceId,
            [Substitute] ILambdaContext context
        )
        {
            using var generation = await project.GenerateAssembly();
            using var server = new InMemoryServer();

            var requestType = CustomResourceRequestType.Update;
            var (assembly, _) = generation;
            var handler = new HandlerWrapper<object>(assembly, handlerTypeName);
            var request = CreateRequest(assembly, name);
            request.RequestType = requestType;
            request.ResponseURL = new Uri(server.Address);
            request.StackId = stackId;
            request.RequestId = requestId;
            request.LogicalResourceId = logicalResourceId;
            request.ResourceProperties.Serial = 1UL;
            request.OldResourceProperties.Serial = 0UL;

            using var inputStream = await StreamUtils.CreateJsonStream(request);
            await handler.Run(inputStream, context);

            var httpRequest = server.Requests
            .Should()
            .Contain(request =>
                request.HttpMethod == "PUT"
            )
            .Which;

            // Tests to make sure status is serialized to all caps
            httpRequest.Body.Should().MatchRegex("\"Status\":[ ]?\"SUCCESS\"");

            var body = JsonSerializer.Deserialize<CustomResourceResponse<ResponseData>>(httpRequest.Body);
            body.Should().Match<CustomResourceResponse<ResponseData>>(response =>
                response.PhysicalResourceId == name &&
                response.StackId == stackId &&
                response.RequestId == requestId &&
                response.LogicalResourceId == logicalResourceId &&
                response.Status == CustomResourceResponseStatus.Success &&
                response.Data!.MethodCalled == "Create"
            );
        }

        [Test, Auto]
        public async Task Run_PerformsCreateIfUpdateRequiresReplacement_ForNestedTypes(
            string name,
            string requestId,
            string stackId,
            string logicalResourceId,
            [Substitute] ILambdaContext context
        )
        {
            using var generation = await project.GenerateAssembly();
            using var server = new InMemoryServer();

            var requestType = CustomResourceRequestType.Update;
            var (assembly, _) = generation;

            var handler = new HandlerWrapper<object>(assembly, handlerTypeName);
            var request = CreateRequest(assembly, name);
            request.RequestType = requestType;
            request.ResponseURL = new Uri(server.Address);
            request.StackId = stackId;
            request.RequestId = requestId;
            request.LogicalResourceId = logicalResourceId;
            request.ResourceProperties.Metadata.CreatedBy = "Joe New";
            request.OldResourceProperties.Metadata.CreatedBy = "Joe Old";

            using var inputStream = await StreamUtils.CreateJsonStream(request);
            await handler.Run(inputStream, context);

            var httpRequest = server.Requests
            .Should()
            .Contain(request =>
                request.HttpMethod == "PUT"
            )
            .Which;

            // Tests to make sure status is serialized to all caps
            httpRequest.Body.Should().MatchRegex("\"Status\":[ ]?\"SUCCESS\"");

            var body = JsonSerializer.Deserialize<CustomResourceResponse<ResponseData>>(httpRequest.Body);
            body.Should().Match<CustomResourceResponse<ResponseData>>(response =>
                response.PhysicalResourceId == name &&
                response.StackId == stackId &&
                response.RequestId == requestId &&
                response.LogicalResourceId == logicalResourceId &&
                response.Status == CustomResourceResponseStatus.Success &&
                response.Data!.MethodCalled == "Create"
            );
        }

        [Test, Auto]
        public async Task Run_SendsFailure_IfNameIsLessThan3Chars(
            string requestId,
            string stackId,
            string logicalResourceId,
            CustomResourceRequestType requestType,
            [Substitute] ILambdaContext context
        )
        {
            var name = "ab";
            using var generation = await project.GenerateAssembly();
            using var server = new InMemoryServer();

            var (assembly, _) = generation;
            var handler = new HandlerWrapper<object>(assembly, handlerTypeName);
            var request = CreateRequest(assembly, name);
            request.RequestType = requestType;
            request.ResponseURL = new Uri(server.Address);
            request.StackId = stackId;
            request.RequestId = requestId;
            request.LogicalResourceId = logicalResourceId;
            request.PhysicalResourceId = null;

            using var inputStream = await StreamUtils.CreateJsonStream(request);
            await handler.Run(inputStream, context);

            var httpRequest = server.Requests
            .Should()
            .Contain(request =>
                request.HttpMethod == "PUT"
            )
            .Which;

            var body = JsonSerializer.Deserialize<CustomResourceResponse<ResponseData>>(httpRequest.Body);
            body.Should().Match<CustomResourceResponse<ResponseData>>(response =>
                response.StackId == stackId &&
                response.RequestId == requestId &&
                response.LogicalResourceId == logicalResourceId &&
                response.Status == CustomResourceResponseStatus.Failed &&
                response.Reason == "Expected Error Message"
            );
        }

        [Test(Description = "[LAMBJ-129] Old Resource Properties will not be present if the RequestType = Create, therefore they should not be validated.")]
        [Auto]
        public async Task Run_ShouldNotSendFailure_IfOldNameIsLessThan3Chars(
            string requestId,
            string stackId,
            string logicalResourceId,
            CustomResourceRequestType requestType,
            [Substitute] ILambdaContext context
        )
        {
            var name = "abcde";
            var oldName = "a";
            using var generation = await project.GenerateAssembly();
            using var server = new InMemoryServer();

            var (assembly, _) = generation;
            var handler = new HandlerWrapper<object>(assembly, handlerTypeName);
            var request = CreateRequest(assembly, name);
            request.RequestType = requestType;
            request.ResponseURL = new Uri(server.Address);
            request.StackId = stackId;
            request.RequestId = requestId;
            request.LogicalResourceId = logicalResourceId;
            request.PhysicalResourceId = null;
            request.OldResourceProperties.Name = oldName;

            var inputStream = await StreamUtils.CreateJsonStream(request);
            await handler.Run(inputStream, context);

            var httpRequest = server.Requests
            .Should()
            .Contain(request =>
                request.HttpMethod == "PUT"
            )
            .Which;

            var body = JsonSerializer.Deserialize<CustomResourceResponse<ResponseData>>(httpRequest.Body);
            body.Should().Match<CustomResourceResponse<ResponseData>>(response =>
                response.StackId == stackId &&
                response.RequestId == requestId &&
                response.LogicalResourceId == logicalResourceId &&
                response.Status == CustomResourceResponseStatus.Success
            );
        }
    }
}
