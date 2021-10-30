using Amazon.Runtime;

namespace Lambdajection.CompilationTests.Tracing
{
    public class ConcreteClientConfig : ClientConfig
    {
        public override void Validate()
        {
        }

        public override string UserAgent => string.Empty;

        public override string ServiceVersion => string.Empty;

        public override string RegionEndpointServiceName => "concrete";
    }
}
