using System.Text.Json;

using FluentAssertions;

using NUnit.Framework;

namespace Lambdajection.CustomResource.Tests
{
    [Category("Unit")]
    public class CustomResourceRequestTypeTests
    {
        [Test, Auto]
        public void ShouldDeserializeCreate()
        {
            var source = "\"Create\"";
            var result = JsonSerializer.Deserialize<CustomResourceRequestType>(source);

            result.Should().Be(CustomResourceRequestType.Create);
        }

        [Test, Auto]
        public void ShouldDeserializeUpdate()
        {
            var source = "\"Update\"";
            var result = JsonSerializer.Deserialize<CustomResourceRequestType>(source);

            result.Should().Be(CustomResourceRequestType.Update);
        }

        [Test, Auto]
        public void ShouldDeserializeDelete()
        {
            var source = "\"Delete\"";
            var result = JsonSerializer.Deserialize<CustomResourceRequestType>(source);

            result.Should().Be(CustomResourceRequestType.Delete);
        }
    }
}
