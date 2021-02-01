using System;
using System.Text.Json;

using FluentAssertions;

using NUnit.Framework;

namespace Lambdajection.CustomResource.Tests
{
    [Category("Unit")]
    public class CustomResourceResponseStatusConverterTests
    {
        [TestFixture, Category("Unit")]
        public class Read
        {
            [Test, Auto]
            public void ShouldReadSuccess(
                CustomResourceResponseStatusConverter converter,
                JsonSerializerOptions options
            )
            {
                options.Converters.Add(converter);
                var result = JsonSerializer.Deserialize<CustomResourceResponseStatus>("\"Success\"", options);

                result.Should().Be(CustomResourceResponseStatus.Success);
            }

            [Test, Auto]
            public void ShouldReadFailed(
                CustomResourceResponseStatusConverter converter,
                JsonSerializerOptions options
            )
            {
                options.Converters.Add(converter);
                var result = JsonSerializer.Deserialize<CustomResourceResponseStatus>("\"Failed\"", options);

                result.Should().Be(CustomResourceResponseStatus.Failed);
            }

            [Test, Auto]
            public void ShouldThrowForEverythingElse(
                string randomString,
                CustomResourceResponseStatusConverter converter,
                JsonSerializerOptions options
            )
            {
                options.Converters.Add(converter);
                Action func = () => JsonSerializer.Deserialize<CustomResourceResponseStatus>($"\"{randomString}\"", options);

                func.Should().Throw<NotSupportedException>();
            }
        }

        [TestFixture, Category("Unit")]
        public class Write
        {
            [Test, Auto]
            public void ShouldConvertSuccessToUpper(
                CustomResourceResponseStatusConverter converter,
                JsonSerializerOptions options
            )
            {
                var success = CustomResourceResponseStatus.Success;
                options.Converters.Add(converter);

                var result = JsonSerializer.Serialize(success, options);

                result.Should().Be("\"SUCCESS\"");
            }

            [Test, Auto]
            public void ShouldConvertFailedToUpper(
                CustomResourceResponseStatusConverter converter,
                JsonSerializerOptions options
            )
            {
                var success = CustomResourceResponseStatus.Failed;
                options.Converters.Add(converter);

                var result = JsonSerializer.Serialize(success, options);

                result.Should().Be("\"FAILED\"");
            }
        }
    }
}
