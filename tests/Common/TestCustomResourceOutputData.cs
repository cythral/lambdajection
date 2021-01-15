using Lambdajection.CustomResource;

namespace Lambdajection
{
    public class TestCustomResourceOutputData : ICustomResourceOutputData
    {
        public virtual string Id { get; set; } = string.Empty;
    }
}
