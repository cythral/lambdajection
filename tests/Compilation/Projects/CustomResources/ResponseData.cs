using Lambdajection.CustomResource;

namespace Lambdajection.CompilationTests.CustomResources
{
    public class ResponseData : ICustomResourceOutputData
    {
        public string Id { get; set; }

        public string MethodCalled { get; set; }
    }
}
