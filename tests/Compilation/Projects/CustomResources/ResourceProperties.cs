namespace Lambdajection.CompilationTests.CustomResources
{
    public class ResourceProperties
    {
        public string Name { get; set; }

        public bool ShouldFail { get; set; } = false;

        public string ErrorMessage { get; set; } = string.Empty;
    }
}
