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

        private static Project project = null!;

        public static dynamic CreateRequest(Assembly assembly, string name, bool shouldFail = false, string? errorMessage = null)
        {
            var resourcePropertiesType = assembly.GetType(resourcePropertiesTypeName)!;
            dynamic resourceProperties = Activator.CreateInstance(resourcePropertiesType)!;
            resourceProperties.Name = name;
            resourceProperties.ShouldFail = shouldFail;
            resourceProperties.ErrorMessage = errorMessage;

            var requestType = typeof(CustomResourceRequest<>).MakeGenericType(resourcePropertiesType);
            dynamic request = Activator.CreateInstance(requestType)!;
            request.ResourceProperties = resourceProperties;

            return request;
        }

        [OneTimeSetUp]
        public async Task Setup()
        {
            project = await MSBuildProjectExtensions.LoadProject(projectPath);
        }

        [Test, Auto]
        public async Task Run_PerformsOperation(
            string name,
            string requestId,
            string stackId,
            string logicalResourceId,
            CustomResourceRequestType requestType,
            [Substitute] ILambdaContext context
        )
        {
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

            await handler.Run(request, context);

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
        public async Task Run_SendsFailure(
            string name,
            string requestId,
            string stackId,
            string logicalResourceId,
            string errorMessage,
            CustomResourceRequestType requestType,
            [Substitute] ILambdaContext context
        )
        {
            using var generation = await project.GenerateAssembly();
            using var server = new InMemoryServer();

            var (assembly, _) = generation;
            var handler = new HandlerWrapper<object>(assembly, handlerTypeName);
            var request = CreateRequest(assembly, name, true, errorMessage);
            request.RequestType = requestType;
            request.ResponseURL = new Uri(server.Address);
            request.StackId = stackId;
            request.RequestId = requestId;
            request.LogicalResourceId = logicalResourceId;

            await handler.Run(request, context);

            var httpRequest = server.Requests
            .Should()
            .Contain(request =>
                request.HttpMethod == "PUT"
            )
            .Which;

            // Tests to make sure status is serialized to all caps
            httpRequest.Body.Should().MatchRegex("\"Status\":[ ]?\"FAILED\"");

            var body = JsonSerializer.Deserialize<CustomResourceResponse<ResponseData>>(httpRequest.Body);
            body.Should().Match<CustomResourceResponse<ResponseData>>(response =>
                response.StackId == stackId &&
                response.RequestId == requestId &&
                response.LogicalResourceId == logicalResourceId &&
                response.Status == CustomResourceResponseStatus.Failed &&
                response.Reason == errorMessage
            );
        }
    }
}
