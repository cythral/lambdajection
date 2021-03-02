using System;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;

using FluentAssertions;

using Lambdajection.Core;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

using TestCustomResourceLambdaHost = Lambdajection.CustomResource.CustomResourceLambdaHost<
    Lambdajection.TestCustomResourceLambda,
    object,
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
                });

                request.RequestType = CustomResourceRequestType.Create;
                await host.InvokeLambda(request, cancellationToken);

                await lambda.DidNotReceiveWithAnyArgs().Update(default!, default);
                await lambda.DidNotReceiveWithAnyArgs().Delete(default!, default);
                await lambda.Received().Create(Is(request), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldCallUpdate_IfRequestTypeIsUpdate(
                ServiceCollection serviceCollection,
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
                });

                request.RequestType = CustomResourceRequestType.Update;
                await host.InvokeLambda(request, cancellationToken);

                await lambda.DidNotReceiveWithAnyArgs().Create(default!, default);
                await lambda.DidNotReceiveWithAnyArgs().Delete(default!, default);
                await lambda.Received().Update(Is(request), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldCallCreate_IfRequestTypeIsUpdate_ButLambdaRequiresReplacement(
                ServiceCollection serviceCollection,
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
                });

                request.RequestType = CustomResourceRequestType.Update;
                lambda.RequiresReplacement(Any<CustomResourceRequest<object>>()).Returns(true);

                await host.InvokeLambda(request, cancellationToken);

                await lambda.DidNotReceiveWithAnyArgs().Delete(default!, default);
                await lambda.DidNotReceiveWithAnyArgs().Update(default!, default);
                await lambda.Received().Create(Is(request), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldCallDelete_IfRequestTypeIsDelete(
                ServiceCollection serviceCollection,
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
                });

                request.RequestType = CustomResourceRequestType.Delete;
                await host.InvokeLambda(request, cancellationToken);

                await lambda.DidNotReceiveWithAnyArgs().Create(default!, default);
                await lambda.DidNotReceiveWithAnyArgs().Update(default!, default);
                await lambda.Received().Delete(Is(request), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldRespondSuccessWithData(
                ServiceCollection serviceCollection,
                TestCustomResourceOutputData data,
                CustomResourceRequest<object> request,
                [Substitute] TestCustomResourceLambda lambda,
                [Substitute] IHttpClient httpClient
            )
            {
                lambda.Create(Any<CustomResourceRequest<object>>(), Any<CancellationToken>()).Returns(data);
                serviceCollection.AddSingleton(httpClient);

                var serviceProvider = serviceCollection.BuildServiceProvider();
                var cancellationToken = new CancellationToken(false);
                var host = new TestCustomResourceLambdaHost(lambdaHost =>
                {
                    lambdaHost.Lambda = lambda;
                    lambdaHost.Scope = serviceProvider.CreateScope();
                });

                request.RequestType = CustomResourceRequestType.Create;
                await host.InvokeLambda(request, cancellationToken);

                await httpClient.Received().PutJson(
                    Is(request.ResponseURL),
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
                });

                request.RequestType = CustomResourceRequestType.Create;
                request.StackId = stackId;
                await host.InvokeLambda(request, cancellationToken);

                await httpClient.Received().PutJson(
                    Is(request.ResponseURL),
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
                });

                request.RequestType = CustomResourceRequestType.Create;
                request.RequestId = requestId;
                await host.InvokeLambda(request, cancellationToken);

                await httpClient.Received().PutJson(
                    Is(request.ResponseURL),
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
                });

                request.RequestType = CustomResourceRequestType.Create;
                request.LogicalResourceId = logicalResourceId;
                await host.InvokeLambda(request, cancellationToken);

                await httpClient.Received().PutJson(
                    Is(request.ResponseURL),
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
                TestCustomResourceOutputData data,
                CustomResourceRequest<object> request,
                [Substitute] TestCustomResourceLambda lambda,
                [Substitute] IHttpClient httpClient
            )
            {
                data.Id = physicalResourceId;
                lambda.Create(Any<CustomResourceRequest<object>>(), Any<CancellationToken>()).Returns(data);
                serviceCollection.AddSingleton(httpClient);

                var serviceProvider = serviceCollection.BuildServiceProvider();
                var cancellationToken = new CancellationToken(false);
                var host = new TestCustomResourceLambdaHost(lambdaHost =>
                {
                    lambdaHost.Lambda = lambda;
                    lambdaHost.Scope = serviceProvider.CreateScope();
                });

                request.RequestType = CustomResourceRequestType.Create;
                await host.InvokeLambda(request, cancellationToken);

                await httpClient.Received().PutJson(
                    Is(request.ResponseURL),
                    Is<CustomResourceResponse<TestCustomResourceOutputData>>(response =>
                        response.Status == CustomResourceResponseStatus.Success &&
                        response.PhysicalResourceId == physicalResourceId
                    ),
                    Is((string)null!),
                    Is(cancellationToken)
                );
            }

            [Test, Auto]
            public async Task ShouldRespondFailureWithReason(
                string reason,
                ServiceCollection serviceCollection,
                TestCustomResourceOutputData data,
                CustomResourceRequest<object> request,
                [Substitute] TestCustomResourceLambda lambda,
                [Substitute] IHttpClient httpClient
            )
            {
                serviceCollection.AddSingleton(httpClient);
                lambda.Create(Any<CustomResourceRequest<object>>(), Any<CancellationToken>()).Returns<TestCustomResourceOutputData>(x =>
                {
                    throw new Exception(reason);
                });

                var serviceProvider = serviceCollection.BuildServiceProvider();
                var cancellationToken = new CancellationToken(false);
                var host = new TestCustomResourceLambdaHost(lambdaHost =>
                {
                    lambdaHost.Lambda = lambda;
                    lambdaHost.Scope = serviceProvider.CreateScope();
                });

                request.RequestType = CustomResourceRequestType.Create;
                await host.InvokeLambda(request, cancellationToken);

                await httpClient.Received().PutJson(
                    Is(request.ResponseURL),
                    Is<CustomResourceResponse<TestCustomResourceOutputData>>(response =>
                        response.Status == CustomResourceResponseStatus.Failed &&
                        response.Reason == reason
                    ),
                    Is((string)null!),
                    Is(cancellationToken)
                );
            }

            [Test, Auto]
            public async Task ShouldRespondFailureWithRequestId(
                string requestId,
                ServiceCollection serviceCollection,
                TestCustomResourceOutputData data,
                CustomResourceRequest<object> request,
                [Substitute] TestCustomResourceLambda lambda,
                [Substitute] IHttpClient httpClient
            )
            {
                serviceCollection.AddSingleton(httpClient);
                lambda.Create(Any<CustomResourceRequest<object>>(), Any<CancellationToken>()).Returns<TestCustomResourceOutputData>(x =>
                {
                    throw new Exception();
                });

                var serviceProvider = serviceCollection.BuildServiceProvider();
                var cancellationToken = new CancellationToken(false);
                var host = new TestCustomResourceLambdaHost(lambdaHost =>
                {
                    lambdaHost.Lambda = lambda;
                    lambdaHost.Scope = serviceProvider.CreateScope();
                });

                request.RequestType = CustomResourceRequestType.Create;
                request.RequestId = requestId;
                await host.InvokeLambda(request, cancellationToken);

                await httpClient.Received().PutJson(
                    Is(request.ResponseURL),
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
                TestCustomResourceOutputData data,
                CustomResourceRequest<object> request,
                [Substitute] TestCustomResourceLambda lambda,
                [Substitute] IHttpClient httpClient
            )
            {
                serviceCollection.AddSingleton(httpClient);
                lambda.Create(Any<CustomResourceRequest<object>>(), Any<CancellationToken>()).Returns<TestCustomResourceOutputData>(x =>
                {
                    throw new Exception();
                });

                var serviceProvider = serviceCollection.BuildServiceProvider();
                var cancellationToken = new CancellationToken(false);
                var host = new TestCustomResourceLambdaHost(lambdaHost =>
                {
                    lambdaHost.Lambda = lambda;
                    lambdaHost.Scope = serviceProvider.CreateScope();
                });

                request.RequestType = CustomResourceRequestType.Create;
                request.StackId = stackId;
                await host.InvokeLambda(request, cancellationToken);

                await httpClient.Received().PutJson(
                    Is(request.ResponseURL),
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
                CustomResourceRequest<object> request,
                [Substitute] TestCustomResourceLambda lambda,
                [Substitute] IHttpClient httpClient
            )
            {
                serviceCollection.AddSingleton(httpClient);
                lambda.Create(Any<CustomResourceRequest<object>>(), Any<CancellationToken>()).Returns<TestCustomResourceOutputData>(x =>
                {
                    throw new Exception();
                });

                var serviceProvider = serviceCollection.BuildServiceProvider();
                var cancellationToken = new CancellationToken(false);
                var host = new TestCustomResourceLambdaHost(lambdaHost =>
                {
                    lambdaHost.Lambda = lambda;
                    lambdaHost.Scope = serviceProvider.CreateScope();
                });

                request.RequestType = CustomResourceRequestType.Create;
                request.LogicalResourceId = logicalResourceId;
                await host.InvokeLambda(request, cancellationToken);

                await httpClient.Received().PutJson(
                    Is(request.ResponseURL),
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
                CustomResourceRequest<object> request,
                [Substitute] TestCustomResourceLambda lambda,
                [Substitute] IHttpClient httpClient
            )
            {
                serviceCollection.AddSingleton(httpClient);
                lambda.Create(Any<CustomResourceRequest<object>>(), Any<CancellationToken>()).Returns<TestCustomResourceOutputData>(x =>
                {
                    throw new Exception();
                });

                var serviceProvider = serviceCollection.BuildServiceProvider();
                var cancellationToken = new CancellationToken(false);
                var host = new TestCustomResourceLambdaHost(lambdaHost =>
                {
                    lambdaHost.Lambda = lambda;
                    lambdaHost.Scope = serviceProvider.CreateScope();
                });

                request.RequestType = CustomResourceRequestType.Create;
                request.PhysicalResourceId = physicalResourceId;
                await host.InvokeLambda(request, cancellationToken);

                await httpClient.Received().PutJson(
                    Is(request.ResponseURL),
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
