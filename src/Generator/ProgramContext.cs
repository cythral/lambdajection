using System.Collections.Generic;

using Lambdajection.Generator.TemplateGeneration;

using Microsoft.CodeAnalysis;

namespace Lambdajection.Generator
{
    public class ProgramContext
    {
        public GeneratorExecutionContext GeneratorExecutionContext { get; set; }

        public List<LambdaInfo> LambdaInfos { get; set; } = new List<LambdaInfo>();
    }
}
