using System.Collections.Generic;

namespace Lambdajection.Generator.TemplateGeneration
{
    public class Policy
    {
        public Policy(string policyName)
        {
            PolicyName = policyName;
        }

        public string PolicyName { get; set; }

        public PolicyDocument PolicyDocument { get; private set; } = new PolicyDocument();

        public Policy AddStatement(HashSet<string> action, string effect = "Allow", string resource = "*")
        {
            PolicyDocument.Statement.Add(new PolicyStatement()
            {
                Effect = effect,
                Action = action,
                Resource = resource,
                Principal = null,
            });

            return this;
        }
    }
}
