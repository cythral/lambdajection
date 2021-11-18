using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;

using FluentAssertions;

using Lambdajection.Core;
using Lambdajection.Core.Serialization;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

using TestCustomResourceLambdaHost = Lambdajection.CustomResource.CustomResourceLambdaHost<
    Lambdajection.TestCustomResourceLambda,
    Lambdajection.TestLambdaMessage,
    Lambdajection.TestCustomResourceOutputData,
    Lambdajection.TestStartup,
    Lambdajection.TestConfigurator,
    Lambdajection.TestConfigFactory
>;

namespace Lambdajection.CustomResource.Tests
{
    [Category("Unit")]
    public class CustomResourceLambdaHostTests
    {
        [Test]
        public void DefaultConstructorShouldNotThrow()
        {
#pragma warning disable CA1806
            Action func = () => new TestCustomResourceLambda();
            func.Should().NotThrow();
#pragma warning restore CA1806
        }

        [TestFixture, Category("Unit")]
        public class InvokeLambda
        {
            [Test, Auto]
            public async Task ShouldCallCreate_IfRequestTypeIsCreate(
                ServiceCollection serviceCollection,
                CustomResourceRequest<object> request,
                JsonSerializer serializer,
                [Substitute] TestCustomResourceLambda lambda,
                [Substitute] IHttpClient httpClient
            )
            {
                serviceCollection.AddSingleton<ISerializer>(serializer);
                serviceCollection.AddSingleton(httpClient);

                var serviceProvider = serviceCollection.BuildServiceProvider();
                var cancellationToken = new CancellationToken(false);
                var host = new TestCustomResourceLambdaHost(lambdaHost =>
                {
                    lambdaHost.Lambda = lambda;
                    lambdaHost.Scope = serviceProvider.CreateScope();
                    lambdaHost.Serializer = serializer;
                });

                request.RequestType = CustomResourceRequestType.Create;

                using var inputStream = await StreamUtils.CreateJsonStream(request);
                await host.InvokeLambda(inputStream, cancellationToken);

                await lambda.DidNotReceiveWithAnyArgs().Update(default!, default);
                await lambda.DidNotReceiveWithAnyArgs().Delete(default!, default);
                await lambda.Received().Create(Is<CustomResourceRequest<TestLambdaMessage>>(req => req.RequestId == request.RequestId), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldCallUpdate_IfRequestTypeIsUpdate(
                ServiceCollection serviceCollection,
                CustomResourceRequest<object> request,
                JsonSerializer serializer,
                [Substitute] TestCustomResourceLambda lambda,
                [Substitute] IHttpClient httpClient
            )
            {
                serviceCollection.AddSingleton(httpClient);

                var serviceProvider = serviceCollection.BuildServiceProvider();
                var cancellationToken = new CancellationToken(false);
                var host = new TestCustomResourceLambdaHost(lambdaHost =>
                {
                    lambdaHost.Lambda = lambda;
                    lambdaHost.Scope = serviceProvider.CreateScope();
                    lambdaHost.Serializer = serializer;
                });

                request.RequestType = CustomResourceRequestType.Update;

                using var inputStream = await StreamUtils.CreateJsonStream(request);
                await host.InvokeLambda(inputStream, cancellationToken);

                await lambda.DidNotReceiveWithAnyArgs().Create(default!, default);
                await lambda.DidNotReceiveWithAnyArgs().Delete(default!, default);
                await lambda.Received().Update(Is<CustomResourceRequest<TestLambdaMessage>>(req => req.RequestId == request.RequestId), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldCallCreate_IfRequestTypeIsUpdate_ButLambdaRequiresReplacement(
                ServiceCollection serviceCollection,
                JsonSerializer serializer,
                CustomResourceRequest<object> request,
                [Substitute] TestCustomResourceLambda lambda,
                [Substitute] IHttpClient httpClient
            )
            {
                serviceCollection.AddSingleton(httpClient);

                var serviceProvider = serviceCollection.BuildServiceProvider();
                var cancellationToken = new CancellationToken(false);
                var host = new TestCustomResourceLambdaHost(lambdaHost =>
                {
                    lambdaHost.Lambda = lambda;
                    lambdaHost.Scope = serviceProvider.CreateScope();
                    lambdaHost.Serializer = serializer;
                });

                request.RequestType = CustomResourceRequestType.Update;
                lambda.RequiresReplacement(Any<CustomResourceRequest<TestLambdaMessage>>()).Returns(true);

                using var inputStream = await StreamUtils.CreateJsonStream(request);
                await host.InvokeLambda(inputStream, cancellationToken);

                await lambda.DidNotReceiveWithAnyArgs().Delete(default!, default);
                await lambda.DidNotReceiveWithAnyArgs().Update(default!, default);
                await lambda.Received().Create(Is<CustomResourceRequest<TestLambdaMessage>>(req => req.RequestId == request.RequestId), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldCallDelete_IfRequestTypeIsDelete(
                ServiceCollection serviceCollection,
                CustomResourceRequest<object> request,
                JsonSerializer serializer,
                [Substitute] TestCustomResourceLambda lambda,
                [Substitute] IHttpClient httpClient
            )
            {
                serviceCollection.AddSingleton(httpClient);

                var serviceProvider = serviceCollection.BuildServiceProvider();
                var cancellationToken = new CancellationToken(false);
                var host = new TestCustomResourceLambdaHost(lambdaHost =>
                {
                    lambdaHost.Lambda = lambda;
                    lambdaHost.Scope = serviceProvider.CreateScope();
                    lambdaHost.Serializer = serializer;
                });

                request.RequestType = CustomResourceRequestType.Delete;

                using var inputStream = await StreamUtils.CreateJsonStream(request);
                await host.InvokeLambda(inputStream, cancellationToken);

                await lambda.DidNotReceiveWithAnyArgs().Create(default!, default);
                await lambda.DidNotReceiveWithAnyArgs().Update(default!, default);
                await lambda.Received().Delete(Is<CustomResourceRequest<TestLambdaMessage>>(req => req.RequestId == request.RequestId), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldRespondSuccessWithData(
                ServiceCollection serviceCollection,
                TestCustomResourceOutputData data,
                CustomResourceRequest<object> request,
                JsonSerializer serializer,
                [Substitute] TestCustomResourceLambda lambda,
                [Substitute] IHttpClient httpClient
            )
            {
                lambda.Create(Any<CustomResourceRequest<TestLambdaMessage>>(), Any<CancellationToken>()).Returns(data);
                serviceCollection.AddSingleton(httpClient);

                var serviceProvider = serviceCollection.BuildServiceProvider();
                var cancellationToken = new CancellationToken(false);
                var host = new TestCustomResourceLambdaHost(lambdaHost =>
                {
                    lambdaHost.Lambda = lambda;
                    lambdaHost.Scope = serviceProvider.CreateScope();
                    lambdaHost.Serializer = serializer;
                });

                request.RequestType = CustomResourceRequestType.Create;

                using var inputStream = await StreamUtils.CreateJsonStream(request);
                await host.InvokeLambda(inputStream, cancellationToken);

                await httpClient.Received().PutJson(
                    Is(request.ResponseURL!),
                    Is<CustomResourceResponse<TestCustomResourceOutputData>>(response =>
                        response.Status == CustomResourceResponseStatus.Success &&
                        response.Data == data
                    ),
                    Is((string)null!),
                    Is(cancellationToken)
                );
            }

            [Test, Auto]
            public async Task ShouldRespondSuccessWithStackId(
                ServiceCollection serviceCollection,
                string stackId,
                JsonSerializer serializer,
                CustomResourceRequest<object> request,
                [Substitute] TestCustomResourceLambda lambda,
                [Substitute] IHttpClient httpClient
            )
            {
                serviceCollection.AddSingleton(httpClient);

                var serviceProvider = serviceCollection.BuildServiceProvider();
                var cancellationToken = new CancellationToken(false);
                var host = new TestCustomResourceLambdaHost(lambdaHost =>
                {
                    lambdaHost.Lambda = lambda;
                    lambdaHost.Scope = serviceProvider.CreateScope();
                    lambdaHost.Serializer = serializer;
                });

                request.RequestType = CustomResourceRequestType.Create;
                request.StackId = stackId;

                using var inputStream = await StreamUtils.CreateJsonStream(request);
                await host.InvokeLambda(inputStream, cancellationToken);

                await httpClient.Received().PutJson(
                    Is(request.ResponseURL!),
                    Is<CustomResourceResponse<TestCustomResourceOutputData>>(response =>
                        response.Status == CustomResourceResponseStatus.Success &&
                        response.StackId == stackId
                    ),
                    Is((string)null!),
                    Is(cancellationToken)
                );
            }

            [Test, Auto]
            public async Task ShouldRespondSuccessWithRequestId(
                ServiceCollection serviceCollection,
                string requestId,
                JsonSerializer serializer,
                CustomResourceRequest<object> request,
                [Substitute] TestCustomResourceLambda lambda,
                [Substitute] IHttpClient httpClient
            )
            {
                serviceCollection.AddSingleton(httpClient);

                var serviceProvider = serviceCollection.BuildServiceProvider();
                var cancellationToken = new CancellationToken(false);
                var host = new TestCustomResourceLambdaHost(lambdaHost =>
                {
                    lambdaHost.Lambda = lambda;
                    lambdaHost.Scope = serviceProvider.CreateScope();
                    lambdaHost.Serializer = serializer;
                });

                request.RequestType = CustomResourceRequestType.Create;
                request.RequestId = requestId;

                using var inputStream = await StreamUtils.CreateJsonStream(request);
                await host.InvokeLambda(inputStream, cancellationToken);

                await httpClient.Received().PutJson(
                    Is(request.ResponseURL!),
                    Is<CustomResourceResponse<TestCustomResourceOutputData>>(response =>
                        response.Status == CustomResourceResponseStatus.Success &&
                        response.RequestId == requestId
                    ),
                    Is((string)null!),
                    Is(cancellationToken)
                );
            }

            [Test, Auto]
            public async Task ShouldRespondSuccessWithLogicalResourceId(
                ServiceCollection serviceCollection,
                string logicalResourceId,
                JsonSerializer serializer,
                CustomResourceRequest<object> request,
                [Substitute] TestCustomResourceLambda lambda,
                [Substitute] IHttpClient httpClient
            )
            {
                serviceCollection.AddSingleton(httpClient);

                var serviceProvider = serviceCollection.BuildServiceProvider();
                var cancellationToken = new CancellationToken(false);
                var host = new TestCustomResourceLambdaHost(lambdaHost =>
                {
                    lambdaHost.Lambda = lambda;
                    lambdaHost.Scope = serviceProvider.CreateScope();
                    lambdaHost.Serializer = serializer;
                });

                request.RequestType = CustomResourceRequestType.Create;
                request.LogicalResourceId = logicalResourceId;

                using var inputStream = await StreamUtils.CreateJsonStream(request);
                await host.InvokeLambda(inputStream, cancellationToken);

                await httpClient.Received().PutJson(
                    Is(request.ResponseURL!),
                    Is<CustomResourceResponse<TestCustomResourceOutputData>>(response =>
                        response.Status == CustomResourceResponseStatus.Success &&
                        response.LogicalResourceId == logicalResourceId
                    ),
                    Is((string)null!),
                    Is(cancellationToken)
                );
            }

            [Test, Auto]
            public async Task ShouldRespondSuccessWithPhysicalResourceId(
                ServiceCollection serviceCollection,
                string physicalResourceId,
                JsonSerializer serializer,
                TestCustomResourceOutputData data,
                CustomResourceRequest<object> request,
                [Substitute] TestCustomResourceLambda lambda,
                [Substitute] IHttpClient httpClient
            )
            {
                data.Id = physicalResourceId;
                lambda.Create(Any<CustomResourceRequest<TestLambdaMessage>>(), Any<CancellationToken>()).Returns(data);
                serviceCollection.AddSingleton(httpClient);

                var serviceProvider = serviceCollection.BuildServiceProvider();
                var cancellationToken = new CancellationToken(false);
                var host = new TestCustomResourceLambdaHost(lambdaHost =>
                {
                    lambdaHost.Lambda = lambda;
                    lambdaHost.Scope = serviceProvider.CreateScope();
                    lambdaHost.Serializer = serializer;
                });

                request.RequestType = CustomResourceRequestType.Create;

                using var inputStream = await StreamUtils.CreateJsonStream(request);
                await host.InvokeLambda(inputStream, cancellationToken);

                await httpClient.Received().PutJson(
                    Is(request.ResponseURL!),
                    Is<CustomResourceResponse<TestCustomResourceOutputData>>(response =>
                        response.Status == CustomResourceResponseStatus.Success &&
                        response.PhysicalResourceId == physicalResourceId
                    ),
                    Is((string)null!),
                    Is(cancellationToken)
                );
            }

            [Test, Auto]
            public async Task ShouldNotRespondSuccessIfResponseURLIsNull(
                ServiceCollection serviceCollection,
                string stackId,
                JsonSerializer serializer,
                CustomResourceRequest<object> request,
                [Substitute] TestCustomResourceLambda lambda,
                [Substitute] IHttpClient httpClient
            )
            {
                serviceCollection.AddSingleton(httpClient);
                request.ResponseURL = null;

                var serviceProvider = serviceCollection.BuildServiceProvider();
                var cancellationToken = new CancellationToken(false);
                var host = new TestCustomResourceLambdaHost(lambdaHost =>
                {
                    lambdaHost.Lambda = lambda;
                    lambdaHost.Scope = serviceProvider.CreateScope();
                    lambdaHost.Serializer = serializer;
                });

                request.RequestType = CustomResourceRequestType.Create;
                request.StackId = stackId;

                using var inputStream = await StreamUtils.CreateJsonStream(request);
                await host.InvokeLambda(inputStream, cancellationToken);

                await httpClient.DidNotReceiveWithAnyArgs().PutJson(
                    default!,
                    default(CustomResourceResponse<TestCustomResourceOutputData>)!,
                    default!,
                    default!
                );
            }

            [Test, Auto]
            public async Task ShouldRespondFailureWithReason(
                string reason,
                ServiceCollection serviceCollection,
                JsonSerializer serializer,
                TestCustomResourceOutputData data,
                CustomResourceRequest<object> request,
                [Substitute] TestCustomResourceLambda lambda,
                [Substitute] IHttpClient httpClient
            )
            {
                serviceCollection.AddSingleton<ISerializer>(serializer);
                serviceCollection.AddSingleton(httpClient);
                lambda.Create(Any<CustomResourceRequest<TestLambdaMessage>>(), Any<CancellationToken>()).Returns<TestCustomResourceOutputData>(x =>
                {
                    throw new Exception(reason);
                });

                var serviceProvider = serviceCollection.BuildServiceProvider();
                var cancellationToken = new CancellationToken(false);
                var host = new TestCustomResourceLambdaHost(lambdaHost =>
                {
                    lambdaHost.Lambda = lambda;
                    lambdaHost.Scope = serviceProvider.CreateScope();
                    lambdaHost.Serializer = serializer;
                });

                request.RequestType = CustomResourceRequestType.Create;

                using var inputStream = await StreamUtils.CreateJsonStream(request);
                await host.InvokeLambda(inputStream, cancellationToken);

                await httpClient.Received().PutJson(
                    Is(request.ResponseURL!),
                    Is<CustomResourceResponse<TestCustomResourceOutputData>>(response =>
                        response.Status == CustomResourceResponseStatus.Failed &&
                        response.Reason == reason
                    ),
                    Is((string)null!),
                    Is(cancellationToken)
                );
            }

            [Test, Auto]
            public async Task ShouldRespondFailureIfRequestDoesntDeserializeProperly(
                string requestId,
                Uri responseURL,
                TestCustomResourceOutputData data,
                JsonSerializer serializer,
                ServiceCollection serviceCollection,
                [Substitute] TestCustomResourceLambda lambda,
                [Substitute] IHttpClient httpClient
            )
            {
                serviceCollection.AddSingleton(httpClient);
                lambda.Create(Any<CustomResourceRequest<TestLambdaMessage>>(), Any<CancellationToken>()).Returns(data);

                var serviceProvider = serviceCollection.BuildServiceProvider();
                var cancellationToken = new CancellationToken(false);
                var host = new TestCustomResourceLambdaHost(lambdaHost =>
                {
                    lambdaHost.Lambda = lambda;
                    lambdaHost.Scope = serviceProvider.CreateScope();
                    lambdaHost.Serializer = serializer;
                });

                var input = $@"{{ ""RequestId"": ""{requestId}"", ""ResponseURL"": ""{responseURL}"", ""RequestType"": ""Create"", ""ResourceProperties"": {{ ""Id"": 1 }} }}";

                using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(input));
                await host.InvokeLambda(inputStream, cancellationToken);

                await httpClient.Received().PutJson(
                    Is(responseURL),
                    Is<CustomResourceResponse<TestCustomResourceOutputData>>(response =>
                        response.Status == CustomResourceResponseStatus.Failed &&
                        response.RequestId == requestId
                    ),
                    Is((string)null!),
                    Is(cancellationToken)
                );
            }

            [Test, Auto]
            public async Task ShouldThrowIfRequestDeserializesToNull(
                string requestId,
                Uri responseURL,
                JsonSerializer serializer,
                TestCustomResourceOutputData data,
                ServiceCollection serviceCollection,
                [Substitute] TestCustomResourceLambda lambda,
                [Substitute] IHttpClient httpClient
            )
            {
                serviceCollection.AddSingleton(httpClient);
                lambda.Create(Any<CustomResourceRequest<TestLambdaMessage>>(), Any<CancellationToken>()).Returns(data);

                var serviceProvider = serviceCollection.BuildServiceProvider();
                var cancellationToken = new CancellationToken(false);
                var host = new TestCustomResourceLambdaHost(lambdaHost =>
                {
                    lambdaHost.Lambda = lambda;
                    lambdaHost.Scope = serviceProvider.CreateScope();
                    lambdaHost.Serializer = serializer;
                });

                var input = $@"null";

                using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(input));
                Func<Task> func = () => host.InvokeLambda(inputStream, cancellationToken);

                await func.Should().ThrowAsync<SerializationException>();
            }

            [Test, Auto]
            public async Task ShouldRespondFailureWithRequestId(
                string requestId,
                ServiceCollection serviceCollection,
                JsonSerializer serializer,
                TestCustomResourceOutputData data,
                CustomResourceRequest<object> request,
                [Substitute] TestCustomResourceLambda lambda,
                [Substitute] IHttpClient httpClient
            )
            {
                serviceCollection.AddSingleton(httpClient);
                lambda.Create(Any<CustomResourceRequest<TestLambdaMessage>>(), Any<CancellationToken>()).Returns<TestCustomResourceOutputData>(x =>
                {
                    throw new Exception();
                });

                var serviceProvider = serviceCollection.BuildServiceProvider();
                var cancellationToken = new CancellationToken(false);
                var host = new TestCustomResourceLambdaHost(lambdaHost =>
                {
                    lambdaHost.Lambda = lambda;
                    lambdaHost.Scope = serviceProvider.CreateScope();
                    lambdaHost.Serializer = serializer;
                });

                request.RequestType = CustomResourceRequestType.Create;
                request.RequestId = requestId;

                using var inputStream = await StreamUtils.CreateJsonStream(request);
                await host.InvokeLambda(inputStream, cancellationToken);

                await httpClient.Received().PutJson(
                    Is(request.ResponseURL!),
                    Is<CustomResourceResponse<TestCustomResourceOutputData>>(response =>
                        response.Status == CustomResourceResponseStatus.Failed &&
                        response.RequestId == requestId
                    ),
                    Is((string)null!),
                    Is(cancellationToken)
                );
            }

            [Test, Auto]
            public async Task ShouldRespondFailureWithStackId(
                string stackId,
                ServiceCollection serviceCollection,
                JsonSerializer serializer,
                TestCustomResourceOutputData data,
                CustomResourceRequest<object> request,
                [Substitute] TestCustomResourceLambda lambda,
                [Substitute] IHttpClient httpClient
            )
            {
                serviceCollection.AddSingleton(httpClient);
                lambda.Create(Any<CustomResourceRequest<TestLambdaMessage>>(), Any<CancellationToken>()).Returns<TestCustomResourceOutputData>(x =>
                {
                    throw new Exception();
                });

                var serviceProvider = serviceCollection.BuildServiceProvider();
                var cancellationToken = new CancellationToken(false);
                var host = new TestCustomResourceLambdaHost(lambdaHost =>
                {
                    lambdaHost.Lambda = lambda;
                    lambdaHost.Scope = serviceProvider.CreateScope();
                    lambdaHost.Serializer = serializer;
                });

                request.RequestType = CustomResourceRequestType.Create;
                request.StackId = stackId;

                using var inputStream = await StreamUtils.CreateJsonStream(request);
                await host.InvokeLambda(inputStream, cancellationToken);

                await httpClient.Received().PutJson(
                    Is(request.ResponseURL!),
                    Is<CustomResourceResponse<TestCustomResourceOutputData>>(response =>
                        response.Status == CustomResourceResponseStatus.Failed &&
                        response.StackId == stackId
                    ),
                    Is((string)null!),
                    Is(cancellationToken)
                );
            }

            [Test, Auto]
            public async Task ShouldRespondFailureWithLogicalResourceId(
                string logicalResourceId,
                ServiceCollection serviceCollection,
                TestCustomResourceOutputData data,
                JsonSerializer serializer,
                CustomResourceRequest<object> request,
                [Substitute] TestCustomResourceLambda lambda,
                [Substitute] IHttpClient httpClient
            )
            {
                serviceCollection.AddSingleton(httpClient);
                lambda.Create(Any<CustomResourceRequest<TestLambdaMessage>>(), Any<CancellationToken>()).Returns<TestCustomResourceOutputData>(x =>
                {
                    throw new Exception();
                });

                var serviceProvider = serviceCollection.BuildServiceProvider();
                var cancellationToken = new CancellationToken(false);
                var host = new TestCustomResourceLambdaHost(lambdaHost =>
                {
                    lambdaHost.Lambda = lambda;
                    lambdaHost.Scope = serviceProvider.CreateScope();
                    lambdaHost.Serializer = serializer;
                });

                request.RequestType = CustomResourceRequestType.Create;
                request.LogicalResourceId = logicalResourceId;

                using var inputStream = await StreamUtils.CreateJsonStream(request);
                await host.InvokeLambda(inputStream, cancellationToken);

                await httpClient.Received().PutJson(
                    Is(request.ResponseURL!),
                    Is<CustomResourceResponse<TestCustomResourceOutputData>>(response =>
                        response.Status == CustomResourceResponseStatus.Failed &&
                        response.LogicalResourceId == logicalResourceId
                    ),
                    Is((string)null!),
                    Is(cancellationToken)
                );
            }

            [Test, Auto]
            public async Task ShouldRespondFailureWithPhysicalResourceId(
                string physicalResourceId,
                ServiceCollection serviceCollection,
                TestCustomResourceOutputData data,
                JsonSerializer serializer,
                CustomResourceRequest<object> request,
                [Substitute] TestCustomResourceLambda lambda,
                [Substitute] IHttpClient httpClient
            )
            {
                serviceCollection.AddSingleton(httpClient);
                lambda.Create(Any<CustomResourceRequest<TestLambdaMessage>>(), Any<CancellationToken>()).Returns<TestCustomResourceOutputData>(x =>
                {
                    throw new Exception();
                });

                var serviceProvider = serviceCollection.BuildServiceProvider();
                var cancellationToken = new CancellationToken(false);
                var host = new TestCustomResourceLambdaHost(lambdaHost =>
                {
                    lambdaHost.Lambda = lambda;
                    lambdaHost.Scope = serviceProvider.CreateScope();
                    lambdaHost.Serializer = serializer;
                });

                request.RequestType = CustomResourceRequestType.Create;
                request.PhysicalResourceId = physicalResourceId;

                using var inputStream = await StreamUtils.CreateJsonStream(request);
                await host.InvokeLambda(inputStream, cancellationToken);

                await httpClient.Received().PutJson(
                    Is(request.ResponseURL!),
                    Is<CustomResourceResponse<TestCustomResourceOutputData>>(response =>
                        response.Status == CustomResourceResponseStatus.Failed &&
                        response.PhysicalResourceId == physicalResourceId
                    ),
                    Is((string)null!),
                    Is(cancellationToken)
                );
            }
        }
    }
}
