using System.Threading.Tasks;

using Amazon.Lambda.Core;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using NUnit.Framework;

using TestLambdaHost = Lambdajection.Core.LambdaHost<
    Lambdajection.TestLambda,
    object,
    object,
    Lambdajection.TestStartup,
    Lambdajection.TestConfigurator
>;

namespace Lambdajection.Core.Tests
{
    [Category("Unit")]
    public class LambdaHostTests
    {
        [Test]
        public async Task RunCreatesLambdaAndCallsHandle()
        {
            var expectedResponse = "expectedResponse";
            var lambda = Substitute.For<TestLambda>();
            lambda.Handle(Arg.Any<object>(), Arg.Any<ILambdaContext>()).Returns(expectedResponse);

            var collection = new ServiceCollection();
            collection.AddSingleton(lambda);

            var provider = collection.BuildServiceProvider();
            var host = new TestLambdaHost(lambdaHost =>
            {
                lambdaHost.ServiceProvider = provider;
            });

            var request = new object();
            var context = Substitute.For<ILambdaContext>();
            var response = await host.Run(request, context);

            response.Should().Be(expectedResponse);
            await lambda.Received().Handle(Arg.Is(request), Arg.Is(context));
        }
    }
}
