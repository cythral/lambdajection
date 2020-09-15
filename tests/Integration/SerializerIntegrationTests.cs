using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;

using FluentAssertions;

using Lambdajection.Attributes;

using NUnit.Framework;

namespace Lambdajection.Tests.Integration.Serialization
{
    public class TestSerializer { }

    [Lambda(typeof(TestStartup), Serializer = typeof(TestSerializer))]
    public partial class TestLambdaWithSerializer
    {
#pragma warning disable CA1822
        public Task<string> Handle(string request, ILambdaContext _)
        {
            return Task.FromResult(request);
        }
#pragma warning restore CA1822
    }

    [Lambda(typeof(TestStartup))]
    public partial class TestLambdaWithoutSerializer
    {
#pragma warning disable CA1822
        public Task<string> Handle(string request, ILambdaContext _)
        {
            return Task.FromResult(request);
        }
#pragma warning restore CA1822
    }

    [Category("Integration")]
    public class SerializerIntegrationTests
    {
        [Test]
        public void RunShouldHaveLambdaSerializerAttributeWithGivenSerializer()
        {
            var runMethod = typeof(TestLambdaWithSerializer).GetMethod("Run")!;
            var attributes = runMethod.GetCustomAttributesData();
            var serializerAttributeQuery = from attribute in attributes where attribute.AttributeType == typeof(LambdaSerializerAttribute) select attribute;
            var serializerAttribute = serializerAttributeQuery.FirstOrDefault();

            serializerAttribute.Should().NotBeNull();

            var serializerType = serializerAttribute.ConstructorArguments[0].Value;
            serializerType.Should().Be(typeof(TestSerializer));
        }

        [Test]
        public void RunShouldHaveLambdaSerializerAttributeWithDefaultSerializerIfNotGiven()
        {
            var runMethod = typeof(TestLambdaWithoutSerializer).GetMethod("Run")!;
            var attributes = runMethod.GetCustomAttributesData();
            var serializerAttributeQuery = from attribute in attributes where attribute.AttributeType == typeof(LambdaSerializerAttribute) select attribute;
            var serializerAttribute = serializerAttributeQuery.FirstOrDefault();

            serializerAttribute.Should().NotBeNull();

            var serializerType = serializerAttribute.ConstructorArguments[0].Value;
            serializerType.Should().Be(typeof(DefaultLambdaJsonSerializer));
        }
    }
}
