using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace Lambdajection.Generator
{
    public class ProgramContext
    {
        public GeneratorExecutionContext GeneratorExecutionContext { get; set; }

        public HashSet<string> ExtraIamPermissionsRequired { get; set; } = new HashSet<string>();
    }
}
