using System;
using System.Text.Json;
using System.Text.RegularExpressions;

using FluentAssertions;

using NUnit.Framework;

namespace Lambdajection.Sns
{
    [Category("Unit")]
    public class CloudFormationStackEventConverterTests
    {
        [TestFixture]
        [Category("Unit")]
        public class Deserialization
        {
            [Test, Auto]
            public void StackIdShouldDeserialize(
                string stackId
            )
            {
                var source = $@"""StackId={stackId}""";
                var result = JsonSerializer.Deserialize<CloudFormationStackEvent>(source);

                result!.StackId.Should().Be(stackId);
            }

            [Test, Auto]
            public void TimestampShouldDeserialize(
                DateTime timestamp
            )
            {
                var timestampString = timestamp.ToString("MM/dd/yyyy HH:mm:ss.fffffff");
                var source = $@"""Timestamp={timestampString}""";
                var result = JsonSerializer.Deserialize<CloudFormationStackEvent>(source);

                result!.Timestamp.Should().Be(timestamp);
            }

            [Test, Auto]
            public void EventIdShouldDeserialize(
                string eventId
            )
            {
                var source = $@"""EventId={eventId}""";
                var result = JsonSerializer.Deserialize<CloudFormationStackEvent>(source);

                result!.EventId.Should().Be(eventId);
            }

            [Test, Auto]
            public void LogicalResourceIdShouldDeserialize(
                string logicalResourceId
            )
            {
                var source = $@"""PhysicalResourceId=test\nLogicalResourceId={logicalResourceId}""";
                var result = JsonSerializer.Deserialize<CloudFormationStackEvent>(source);

                result!.LogicalResourceId.Should().Be(logicalResourceId);
            }

            [Test, Auto]
            public void PhysicalResourceIdShouldDeserialize(
                string physicalResourceId
            )
            {
                var source = $@"""PhysicalResourceId={physicalResourceId}\nLogicalResourceId=test""";
                var result = JsonSerializer.Deserialize<CloudFormationStackEvent>(source);

                result!.PhysicalResourceId.Should().Be(physicalResourceId);
            }

            [Test, Auto]
            public void NamespaceShouldDeserialize(
                string @namespace
            )
            {
                var source = $@"""Namespace={@namespace}""";
                var result = JsonSerializer.Deserialize<CloudFormationStackEvent>(source);

                result!.Namespace.Should().Be(@namespace);
            }

            [Test, Auto]
            public void PrincipalIdShouldDeserialize(
                string principalId
            )
            {
                var source = $@"""PrincipalId={principalId}""";
                var result = JsonSerializer.Deserialize<CloudFormationStackEvent>(source);

                result!.PrincipalId.Should().Be(principalId);
            }

            [Test, Auto]
            public void ResourcePropertiesShouldDeserialize(
                string data
            )
            {
                var source = $"\"ResourceProperties='{{\\\"Data\\\":\\\"{data}\\\"}}'\"";
                var result = JsonSerializer.Deserialize<CloudFormationStackEvent>(source);

                var props = (JsonElement)result!.ResourceProperties!;
                props.GetProperty("Data").GetString().Should().Be(data);
            }

            [Test, Auto]
            public void ResourceStatusShouldDeserialize(
                string resourceStatus
            )
            {
                var source = $@"""ResourceStatus={resourceStatus}""";
                var result = JsonSerializer.Deserialize<CloudFormationStackEvent>(source);

                result!.ResourceStatus.Should().Be(resourceStatus);
            }

            [Test, Auto]
            public void ResourceTypeShouldDeserialize(
               string resourceType
           )
            {
                var source = $@"""ResourceType={resourceType}""";
                var result = JsonSerializer.Deserialize<CloudFormationStackEvent>(source);

                result!.ResourceType.Should().Be(resourceType);
            }

            [Test, Auto]
            public void StackNameShouldDeserialize(
               string stackName
           )
            {
                var source = $@"""StackName={stackName}""";
                var result = JsonSerializer.Deserialize<CloudFormationStackEvent>(source);

                result!.StackName.Should().Be(stackName);
            }

            [Test, Auto]
            public void ClientRequestTokenShouldDeserialize(
               string clientRequestToken
            )
            {
                var source = $@"""ClientRequestToken={clientRequestToken}""";
                var result = JsonSerializer.Deserialize<CloudFormationStackEvent>(source);

                result!.ClientRequestToken.Should().Be(clientRequestToken);
            }

            [Test, Auto]
            public void ShouldDeserializeInsideSnsMessage(
                string stackId,
                string clientRequestToken
            )
            {
                var source = $@"{{""Message"":""ClientRequestToken='{clientRequestToken}'\nStackId='{stackId}'\n""}}";
                var result = JsonSerializer.Deserialize<SnsMessage<CloudFormationStackEvent>>(source);

                result!.Message.ClientRequestToken.Should().Be(clientRequestToken);
                result!.Message.StackId.Should().Be(stackId);
            }
        }

        [TestFixture]
        [Category("Unit")]
        public class Serialization
        {
            [Test, Auto]
            public void StackIdShouldSerialize(
                CloudFormationStackEvent stackEvent
            )
            {
                var result = JsonSerializer.Serialize(stackEvent).Trim('"');
                var lines = Regex.Unescape(result).Split('\n');

                lines.Should().Contain($"StackId='{stackEvent.StackId}'");
            }

            [Test, Auto]
            public void TimestampShouldSerialize(
                CloudFormationStackEvent stackEvent
            )
            {
                var result = JsonSerializer.Serialize(stackEvent).Trim('"');
                var lines = Regex.Unescape(result).Split('\n');

                lines.Should().Contain($"Timestamp='{stackEvent.Timestamp}'");
            }

            [Test, Auto]
            public void EventIdShouldSerialize(
                CloudFormationStackEvent stackEvent
            )
            {
                var result = JsonSerializer.Serialize(stackEvent).Trim('"');
                var lines = Regex.Unescape(result).Split('\n');

                lines.Should().Contain($"EventId='{stackEvent.EventId}'");
            }

            [Test, Auto]
            public void LogicalResourceIdShouldSerialize(
                CloudFormationStackEvent stackEvent
            )
            {
                var result = JsonSerializer.Serialize(stackEvent).Trim('"');
                var lines = Regex.Unescape(result).Split('\n');

                lines.Should().Contain($"LogicalResourceId='{stackEvent.LogicalResourceId}'");
            }

            [Test, Auto]
            public void PhysicalResourceIdShouldSerialize(
                CloudFormationStackEvent stackEvent
            )
            {
                var result = JsonSerializer.Serialize(stackEvent).Trim('"');
                var lines = Regex.Unescape(result).Split('\n');

                lines.Should().Contain($"PhysicalResourceId='{stackEvent.PhysicalResourceId}'");
            }

            [Test, Auto]
            public void NamespaceShouldSerialize(
                CloudFormationStackEvent stackEvent
            )
            {
                var result = JsonSerializer.Serialize(stackEvent).Trim('"');
                var lines = Regex.Unescape(result).Split('\n');

                lines.Should().Contain($"Namespace='{stackEvent.Namespace}'");
            }

            [Test, Auto]
            public void PrincipalIdShouldSerialize(
                CloudFormationStackEvent stackEvent
            )
            {
                var result = JsonSerializer.Serialize(stackEvent).Trim('"');
                var lines = Regex.Unescape(result).Split('\n');

                lines.Should().Contain($"PrincipalId='{stackEvent.PrincipalId}'");
            }

            [Test, Auto]
            public void ResourcePropertiesShouldSerialize(
                CloudFormationStackEvent stackEvent
            )
            {
                var result = JsonSerializer.Serialize(stackEvent).Trim('"');
                var lines = Regex.Unescape(result).Split('\n');

                lines.Should().Contain($"ResourceProperties='{JsonSerializer.Serialize(stackEvent.ResourceProperties)}'");
            }

            [Test, Auto]
            public void ResourceStatusShouldSerialize(
                CloudFormationStackEvent stackEvent
            )
            {
                var result = JsonSerializer.Serialize(stackEvent).Trim('"');
                var lines = Regex.Unescape(result).Split('\n');

                lines.Should().Contain($"ResourceStatus='{stackEvent.ResourceStatus}'");
            }

            [Test, Auto]
            public void ResourceTypeShouldSerialize(
                CloudFormationStackEvent stackEvent
            )
            {
                var result = JsonSerializer.Serialize(stackEvent).Trim('"');
                var lines = Regex.Unescape(result).Split('\n');

                lines.Should().Contain($"ResourceType='{stackEvent.ResourceType}'");
            }

            [Test, Auto]
            public void StackNameShouldSerialize(
                CloudFormationStackEvent stackEvent
            )
            {
                var result = JsonSerializer.Serialize(stackEvent).Trim('"');
                var lines = Regex.Unescape(result).Split('\n');

                lines.Should().Contain($"StackName='{stackEvent.StackName}'");
            }

            [Test, Auto]
            public void ClientRequestTokenShouldSerialize(
                CloudFormationStackEvent stackEvent
            )
            {
                var result = JsonSerializer.Serialize(stackEvent).Trim('"');
                var lines = Regex.Unescape(result).Split('\n');

                lines.Should().Contain($"ClientRequestToken='{stackEvent.ClientRequestToken}'");
            }

            [Test, Auto]
            public void ShouldSerializeInsideSnsMessage(
                string stackId,
                string clientRequestToken,
                SnsMessage<CloudFormationStackEvent> message
            )
            {
                var serialized = JsonSerializer.Serialize(message);
                Console.WriteLine(serialized);
                var deserialized = JsonSerializer.Deserialize<SnsMessage<CloudFormationStackEvent>>(serialized);

                deserialized!.Message.StackId.Should().Be(message.Message.StackId);
            }
        }
    }
}
