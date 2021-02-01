using System.Text.Json.Serialization;

namespace Lambdajection.CustomResource
{
    /// <summary>
    /// Describes the type of custom resource request
    /// sent by CloudFormation.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CustomResourceRequestType
    {
        /// <summary>Request Type used to create a Custom Resource.</summary>
        Create,

        /// <summary>Request Type used to update a Custom Resource.</summary>
        Update,

        /// <summary>RequestType used to delete a Custom Resource.</summary>
        Delete,
    }
}
