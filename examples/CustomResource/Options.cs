using Lambdajection.Attributes;

namespace Lambdajection.Examples.CustomResource
{
    [LambdaOptions(typeof(PasswordGenerator), "PasswordGen")]
    public class Options
    {
        public uint DefaultLength { get; set; } = 12;
    }
}
