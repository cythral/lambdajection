using Lambdajection.Attributes;

namespace Lambdajection.CompilationTests.CustomResources
{
    public class Metadata
    {
        [UpdateRequiresReplacement]
        public string CreatedBy { get; set; }
    }
}
