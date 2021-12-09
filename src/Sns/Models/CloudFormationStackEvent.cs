using System;
using System.Text.Json.Serialization;

namespace Lambdajection.Sns
{
    /// <summary>
    /// Represents a CloudFormation stack event sent via SNS.
    /// </summary>
    [JsonConverter(typeof(CloudFormationStackEventConverter))]
    public class CloudFormationStackEvent
    {
        /// <summary>
        /// Gets or sets the id of the stack this event came from.
        /// </summary>
        public string StackId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp of this event.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the id of this event.
        /// </summary>
        public string EventId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the logical resource id pertaining to this event.
        /// </summary>
        public string LogicalResourceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the physical resource id pertaining to this event.
        /// </summary>
        public string PhysicalResourceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the namespace pertaining to this event.
        /// </summary>
        public string Namespace { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the principal id pertaining to this event.
        /// </summary>
        public string PrincipalId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the resource properties associated with this event.
        /// </summary>
        public object? ResourceProperties { get; set; }

        /// <summary>
        /// Gets or sets the resource status associated with this event.
        /// </summary>
        public string ResourceStatus { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the resource type associated with this event.
        /// </summary>
        public string ResourceType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the stack this event originates from.
        /// </summary>
        public string StackName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the client request token pertaining to this event.
        /// </summary>
        public string ClientRequestToken { get; set; } = string.Empty;
    }
}
