using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using YamlDotNet.Serialization;

namespace Lambdajection.Generator.TemplateGeneration
{
    public class TemplateGenerator
    {
        private readonly string templateFilePath;
        private readonly string assemblyName;
        private readonly string codeDirectory;
        private readonly string targetFrameworkVersion;
        private readonly List<LambdaInfo> lambdaInfos;

        public TemplateGenerator(
            string templateFilePath,
            string assemblyName,
            string codeDirectory,
            string targetFrameworkVersion,
            List<LambdaInfo> lambdaInfos
        )
        {
            this.templateFilePath = templateFilePath;
            this.assemblyName = assemblyName;
            this.codeDirectory = codeDirectory;
            this.targetFrameworkVersion = targetFrameworkVersion;
            this.lambdaInfos = lambdaInfos;
        }

        public void GenerateTemplates()
        {
            var serializer = new SerializerBuilder()
                .WithTagMapping("!GetAtt", typeof(GetAttTag))
                .WithTagMapping("!Sub", typeof(SubTag))
                .WithTypeConverter(new GetAttTagConverter())
                .WithTypeConverter(new SubTagConverter())
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                .Build();

            foreach (var lambdaInfo in lambdaInfos)
            {
                var templateLocation = templateFilePath + "/" + lambdaInfo.ClassName + ".template.yml";
                Console.WriteLine("Writing template to " + templateLocation);
                File.WriteAllText(templateLocation, serializer.Serialize(new
                {
                    Resources = GetResourcesForLambda(lambdaInfo),
                    Outputs = GetOutputsForLambda(lambdaInfo),
                }));
            }
        }

        private Dictionary<string, Resource> GetResourcesForLambda(LambdaInfo lambdaInfo)
        {
            var resources = new Dictionary<string, Resource> { };
            resources.Add($"{lambdaInfo.ClassName}Role", GetRoleResourceForLambda(lambdaInfo));

            resources.Add(lambdaInfo.ClassName + "Lambda", new Resource
            {
                Type = "AWS::Lambda::Function",
                Properties = new
                {
                    Handler = $"{assemblyName}::{lambdaInfo.FullyQualifiedClassName}::Run",
                    Role = new GetAttTag() { Name = $"{lambdaInfo.ClassName}Role", Attribute = "Arn" },
                    Code = codeDirectory,
                    Runtime = $"dotnetcore{targetFrameworkVersion}",
                    Timeout = 300,
                },
            });

            return resources;
        }

        private Resource GetRoleResourceForLambda(LambdaInfo lambdaInfo)
        {
            var role = new Role()
            .AddTrustedServiceEntity("lambda.amazonaws.com")
            .AddManagedPolicy("arn:aws:iam::aws:policy/AWSLambdaExecute");

            if (lambdaInfo.Permissions.Any())
            {
                var policy = new Policy($"{lambdaInfo.ClassName}PrimaryPolicy");
                policy.AddStatement(action: lambdaInfo.Permissions);
                role.AddPolicy(policy);
            }

            return role;
        }

        private Dictionary<string, Output> GetOutputsForLambda(LambdaInfo lambdaInfo)
        {
            var outputs = new Dictionary<string, Output> { };

            outputs.Add(lambdaInfo.ClassName + "LambdaArn", new Output(
                value: new GetAttTag { Name = $"{lambdaInfo.ClassName}Lambda", Attribute = "Arn" },
                name: new SubTag { Expression = $"${{AWS::StackName}}:{lambdaInfo.ClassName}LambdaArn" }
            ));

            outputs.Add(lambdaInfo.ClassName + "RoleArn", new Output(
                value: new GetAttTag { Name = $"{lambdaInfo.ClassName}Role", Attribute = "Arn" },
                name: new SubTag { Expression = $"${{AWS::StackName}}:{lambdaInfo.ClassName}RoleArn" }
            ));

            return outputs;
        }
    }
}
