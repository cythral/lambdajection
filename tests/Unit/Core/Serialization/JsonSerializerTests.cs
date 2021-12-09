using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using NUnit.Framework;

namespace Lambdajection.Core.Serialization
{
    [TestFixture]
    [Category("Unit")]
    public class JsonSerializerTests
    {
        [Test, Auto]
        public async Task Serialize_SerializesToJsonStream(
            [Target] JsonSerializer serializer
        )
        {
            var serializable = new TestObject { A = "A", B = "B" };
            var cancellationToken = new CancellationToken(false);
            var stream = new MemoryStream();
            await serializer.Serialize(stream, serializable, cancellationToken);

            using var reader = new StreamReader(stream);
            var result = await reader.ReadToEndAsync();
            result.Should().Be("{\"A\":\"A\",\"B\":\"B\"}");
        }

        [Test, Auto]
        public void Deserialize_DeserializesToObject(
            [Target] JsonSerializer serializer
        )
        {
            var deserializable = "{\"A\":\"A\",\"B\":\"B\"}";
            var result = serializer.Deserialize<TestObject>(deserializable);

            result!.A.Should().Be("A");
            result!.B.Should().Be("B");
        }

        [Test, Auto]
        public async Task Deserialize_DeserializesStreamsToObjects(
            [Target] JsonSerializer serializer
        )
        {
            var deserializableBytes = Encoding.UTF8.GetBytes("{\"A\":\"A\",\"B\":\"B\"}");
            var deserializableStream = new MemoryStream(deserializableBytes);
            var cancellationToken = new CancellationToken(false);

            var result = await serializer.Deserialize<TestObject>(deserializableStream, cancellationToken);

            result!.A.Should().Be("A");
            result!.B.Should().Be("B");
        }

        private class TestObject
        {
            public string A { get; set; } = string.Empty;

            public string B { get; set; } = string.Empty;
        }
    }
}
