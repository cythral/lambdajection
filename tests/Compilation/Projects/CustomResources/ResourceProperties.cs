using System.ComponentModel.DataAnnotations;

namespace Lambdajection.CompilationTests.CustomResources
{
    public class ResourceProperties
    {
        [MinLength(3, ErrorMessage = "Expected Error Message")]
        public string Name { get; set; }

        public bool ShouldFail { get; set; } = false;

        public string ErrorMessage { get; set; } = string.Empty;
    }
}
