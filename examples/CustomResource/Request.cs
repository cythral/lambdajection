using System.ComponentModel.DataAnnotations;

using Lambdajection.Attributes;

namespace Lambdajection.Examples.CustomResource
{
    public class Request
    {
        [Range(3, 12)]
        [UpdateRequiresReplacement]
        public uint? Length { get; set; } = null;
    }
}
