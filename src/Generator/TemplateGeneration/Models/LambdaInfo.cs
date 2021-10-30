using System.Collections.Generic;

namespace Lambdajection.Generator.TemplateGeneration
{
    public class LambdaInfo
    {
        public string ClassName { get; set; }

        public string FullyQualifiedClassName { get; set; }

        public HashSet<string> Permissions { get; set; }

        public bool EnableTracing { get; set; }
    }
}
