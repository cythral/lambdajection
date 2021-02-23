using System.ComponentModel.DataAnnotations;

namespace Lambdajection.Examples.CustomResource
{
    public class Request
    {
        [MinLength(3)]
        public uint? Length { get; set; } = null;
    }
}
