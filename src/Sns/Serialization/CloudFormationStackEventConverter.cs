using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lambdajection.Sns
{
    /// <summary>
    /// Converts a <see cref="CloudFormationStackEvent" /> to and from json.
    /// </summary>
    public class CloudFormationStackEventConverter : JsonConverter<CloudFormationStackEvent>
    {
        /// <inheritdoc />
        public override CloudFormationStackEvent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var source = reader.GetString();
            var lines = source!.Split('\n');
            var result = new CloudFormationStackEvent();

            foreach (var line in lines)
            {
                var delimiterIndex = line.IndexOf('=');
                if (delimiterIndex == -1)
                {
                    continue;
                }

                var key = line[0..delimiterIndex].Trim();
                var value = line[(delimiterIndex + 1)..].Trim('\'');

                switch (key)
                {
                    case nameof(CloudFormationStackEvent.StackId):
                        {
                            result.StackId = value;
                            break;
                        }

                    case nameof(CloudFormationStackEvent.Timestamp):
                        {
                            result.Timestamp = DateTime.Parse(value);
                            break;
                        }

                    case nameof(CloudFormationStackEvent.EventId):
                        {
                            result.EventId = value;
                            break;
                        }

                    case nameof(CloudFormationStackEvent.LogicalResourceId):
                        {
                            result.LogicalResourceId = value;
                            break;
                        }

                    case nameof(CloudFormationStackEvent.PhysicalResourceId):
                        {
                            result.PhysicalResourceId = value;
                            break;
                        }

                    case nameof(CloudFormationStackEvent.Namespace):
                        {
                            result.Namespace = value;
                            break;
                        }

                    case nameof(CloudFormationStackEvent.PrincipalId):
                        {
                            result.PrincipalId = value;
                            break;
                        }

                    case nameof(CloudFormationStackEvent.ResourceProperties):
                        {
                            result.ResourceProperties = JsonSerializer.Deserialize<object>(value);
                            break;
                        }

                    case nameof(CloudFormationStackEvent.ResourceStatus):
                        {
                            result.ResourceStatus = value;
                            break;
                        }

                    case nameof(CloudFormationStackEvent.ResourceType):
                        {
                            result.ResourceType = value;
                            break;
                        }

                    case nameof(CloudFormationStackEvent.StackName):
                        {
                            result.StackName = value;
                            break;
                        }

                    case nameof(CloudFormationStackEvent.ClientRequestToken):
                        {
                            result.ClientRequestToken = value;
                            break;
                        }

                    default: break;
                }
            }

            return result;
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, CloudFormationStackEvent value, JsonSerializerOptions options)
        {
            var lines = string.Empty;
            lines += $"StackId='{value.StackId}'\n";
            lines += $"Timestamp='{value.Timestamp}'\n";
            lines += $"EventId='{value.EventId}'\n";
            lines += $"LogicalResourceId='{value.LogicalResourceId}'\n";
            lines += $"PhysicalResourceId='{value.PhysicalResourceId}'\n";
            lines += $"Namespace='{value.Namespace}'\n";
            lines += $"PrincipalId='{value.PrincipalId}'\n";
            lines += $"ResourceProperties='{JsonSerializer.Serialize(value.ResourceProperties)}'\n";
            lines += $"ResourceStatus='{value.ResourceStatus}'\n";
            lines += $"ResourceType='{value.ResourceType}'\n";
            lines += $"StackName='{value.StackName}'\n";
            lines += $"ClientRequestToken='{value.ClientRequestToken}'\n";

            writer.WriteStringValue(lines);
        }
    }
}
