using System;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture.AutoNSubstitute;

using FluentAssertions;

using Lambdajection.Core.Exceptions;
using Lambdajection.Core.Serialization;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using NUnit.Framework;

using static NSubstitute.Arg;

using ILogger = Microsoft.Extensions.Logging.ILogger;
using TestSnsLambdaHost = Lambdajection.Sns.SnsLambdaHost<
    Lambdajection.TestSnsLambda,
    Lambdajection.TestLambdaMessage,
    Lambdajection.TestLambdaMessage,
    Lambdajection.TestStartup,
    Lambdajection.TestConfigurator,
    Lambdajection.TestConfigFactory
>;

namespace Lambdajection.Sns
{
    public class SnsLambdaHostTests
    {
        [TestFixture, Category("Unit")]
        public class InvokeLambda
        {
            public static SnsMessage<TestLambdaMessage> Matches(SnsMessage<TestLambdaMessage> givenMessage)
            {
                return Is<SnsMessage<TestLambdaMessage>>(message =>
                    message.Message.Id == givenMessage.Message.Id
                );
            }

            [Test, Auto]
            public async Task ShouldThrowIfEventDeserializesToNull(
                ServiceCollection serviceCollection,
                JsonSerializer serializer,
                ILogger logger,
                [Substitute] TestSnsLambda lambda
            )
            {
                serviceCollection.AddSingleton<ISerializer>(serializer);

                var serviceProvider = serviceCollection.BuildServiceProvider();
                var cancellationToken = new CancellationToken(false);
                var host = new TestSnsLambdaHost(lambdaHost =>
                {
                    lambdaHost.Lambda = lambda;
                    lambdaHost.Scope = serviceProvider.CreateScope();
                    lambdaHost.Serializer = serializer;
                    lambdaHost.Logger = logger;
                });

                using var inputStream = await StreamUtils.CreateJsonStream<object?>(null);
                Func<Task> func = () => host.InvokeLambda(inputStream, cancellationToken);

                await func.Should().ThrowAsync<InvalidLambdaParameterException>();
            }

            [Test, Auto]
            public async Task ShouldCallLambdaWithEachMessage(
                SnsRecord<TestLambdaMessage> record,
                ServiceCollection serviceCollection,
                JsonSerializer serializer,
                ILogger logger,
                [Substitute] TestSnsLambda lambda
            )
            {
                var request = new SnsEvent<TestLambdaMessage>(new[] { record });

                serviceCollection.AddSingleton<ISerializer>(serializer);

                var serviceProvider = serviceCollection.BuildServiceProvider();
                var cancellationToken = new CancellationToken(false);
                var host = new TestSnsLambdaHost(lambdaHost =>
                {
                    lambdaHost.Lambda = lambda;
                    lambdaHost.Scope = serviceProvider.CreateScope();
                    lambdaHost.Serializer = serializer;
                    lambdaHost.Logger = logger;
                });

                using var inputStream = await StreamUtils.CreateJsonStream(request);
                await host.InvokeLambda(inputStream, cancellationToken);
                await lambda.Received().Handle(Matches(record.Sns), Is(cancellationToken));
            }

            [Test, Auto]
            public async Task ShouldValidateMessage(
                SnsRecord<TestLambdaMessage> record,
                ServiceCollection serviceCollection,
                JsonSerializer serializer,
                ILogger logger,
                [Substitute] TestSnsLambda lambda
            )
            {
                var request = new SnsEvent<TestLambdaMessage>(new[] { record });

                serviceCollection.AddSingleton<ISerializer>(serializer);

                var serviceProvider = serviceCollection.BuildServiceProvider();
                var cancellationToken = new CancellationToken(false);
                var host = new TestSnsLambdaHost(lambdaHost =>
                {
                    lambdaHost.Lambda = lambda;
                    lambdaHost.Scope = serviceProvider.CreateScope();
                    lambdaHost.Serializer = serializer;
                    lambdaHost.Logger = logger;
                });

                using var inputStream = await StreamUtils.CreateJsonStream(request);
                await host.InvokeLambda(inputStream, cancellationToken);

                lambda.Received().Validate(Matches(record.Sns));
            }
        }
    }
}
