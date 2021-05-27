using System.Collections.Generic;

namespace Lambdajection.Generator.TemplateGeneration
{
    public class PolicyDocument
    {
        public string Version { get; set; } = "2012-10-17";

        public List<PolicyStatement> Statement { get; set; } = new List<PolicyStatement>();
    }
}
