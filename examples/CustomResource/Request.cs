using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Lambdajection.Attributes;

namespace Lambdajection.Examples.CustomResource
{
    public class Request
    {
        [Range(3, 12)]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        [UpdateRequiresReplacement]
        public uint? Length { get; set; } = null;
    }
}
