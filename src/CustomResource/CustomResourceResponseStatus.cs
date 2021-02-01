using System.Text.Json.Serialization;

namespace Lambdajection.CustomResource
{
    /// <summary>
    /// Describes the status of a response returned
    /// from a Custom Resource request.
    /// </summary>
    [JsonConverter(typeof(CustomResourceResponseStatusConverter))]
    public enum CustomResourceResponseStatus
    {
        /// <summary>Describes a success response.</summary>
        Success,

        /// <summary>Describes a failure response.</summary>
        Failed,
    }
}
