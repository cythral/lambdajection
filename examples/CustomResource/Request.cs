using System.ComponentModel.DataAnnotations;

using Lambdajection.Attributes;

namespace Lambdajection.Examples.CustomResource
{
    public class Request
    {
        [MinLength(3)]
        [UpdateRequiresReplacement]
        public uint? Length { get; set; } = null;
    }
}
