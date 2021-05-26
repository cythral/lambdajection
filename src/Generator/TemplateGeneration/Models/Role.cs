using System.Collections.Generic;
using System.Linq;

namespace Lambdajection.Generator.TemplateGeneration
{
    public class Role : Resource
    {
        public override string Type { get; set; } = "AWS::IAM::Role";

        public override object Properties { get; set; } = new PropertiesDefinition();

        public Role AddPolicy(Policy policy)
        {
            ((PropertiesDefinition)Properties).Policies.Add(policy);
            return this;
        }

        public Role AddTrustedAWSEntity(string principal)
        {
            InitializeAssumeRolePolicyDocument();

            var props = (PropertiesDefinition)Properties;
            var trustStatement = props.AssumeRolePolicyDocument.Statement.First();
            trustStatement.Principal ??= new Principal();

            if (trustStatement.Principal.AWS == null)
            {
                trustStatement.Principal.AWS = new HashSet<string>();
            }

            trustStatement.Principal.AWS.Add(principal);
            return this;
        }

        public Role AddTrustedServiceEntity(string principal)
        {
            InitializeAssumeRolePolicyDocument();

            var props = (PropertiesDefinition)Properties;
            var trustStatement = props.AssumeRolePolicyDocument.Statement.First();
            trustStatement.Principal ??= new Principal();

            if (trustStatement.Principal.Service == null)
            {
                trustStatement.Principal.Service = new HashSet<string>();
            }

            trustStatement.Principal.Service.Add(principal);
            return this;
        }

        public Role AddManagedPolicy(string arn)
        {
            ((PropertiesDefinition)Properties).ManagedPolicyArns.Add(arn);
            return this;
        }

        private void InitializeAssumeRolePolicyDocument()
        {
            var props = (PropertiesDefinition)Properties;

            if (props.AssumeRolePolicyDocument == null)
            {
                props.AssumeRolePolicyDocument = new PolicyDocument();
                props.AssumeRolePolicyDocument.Statement.Add(new PolicyStatement()
                {
                    Effect = "Allow",
                    Action = new HashSet<string>() { "sts:AssumeRole" },
                    Principal = new Principal(),
                });
            }
        }

        public class PropertiesDefinition
        {
            public List<string> ManagedPolicyArns { get; set; } = new List<string>();

            public List<Policy> Policies { get; set; } = new List<Policy>();

            public PolicyDocument AssumeRolePolicyDocument { get; set; }
        }
    }
}
