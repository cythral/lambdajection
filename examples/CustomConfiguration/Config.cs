using Lambdajection.Attributes;

namespace Lambdajection.Examples.CustomConfiguration
{
    [LambdaOptions(typeof(Handler), "Config")]
    public class Config
    {
        public string Foo { get; set; } = "";
    }
}
