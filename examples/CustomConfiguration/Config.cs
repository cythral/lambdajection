using Lambdajection.Attributes;

namespace Lambdajection.Examples.ConfigFactory
{
    [LambdaOptions(typeof(Handler), "Config")]
    public class Config
    {
        public string Foo { get; set; } = "";
    }
}
