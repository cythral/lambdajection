using Amazon.Runtime;
using Amazon.Runtime.Internal.Auth;
using Amazon.XRay.Recorder.Handlers.AwsSdk.Internal;

namespace Lambdajection.CompilationTests.Tracing
{
    public class ConcreteAmazonClient : AmazonServiceClient
    {
        public ConcreteAmazonClient() : base(new AnonymousAWSCredentials(), new ConcreteClientConfig())
        {
        }

        protected override AbstractAWSSigner CreateSigner()
        {
            return null!;
        }

        public bool IsTracingEnabled()
        {
            var current = this.RuntimePipeline.Handler;

            while (current != null)
            {
                if (current.GetType() == typeof(XRayPipelineHandler))
                {
                    return true;
                }

                current = current.InnerHandler;
            }

            return false;
        }
    }
}
