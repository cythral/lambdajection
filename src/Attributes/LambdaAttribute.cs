using System;
using System.Diagnostics;

using Cythral.CodeGeneration.Roslyn;

namespace Lambdajection.Attributes
{
    [CodeGenerationAttribute("Lambdajection.Generator.LambdaGenerator, Lambdajection.Generator")]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    [Conditional("CodeGeneration")]
    public class LambdaAttribute : Attribute
    {
        public Type Startup { get; set; } = null!;
    }
}
