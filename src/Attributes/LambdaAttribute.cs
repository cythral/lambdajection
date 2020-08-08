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
        public LambdaAttribute()
        {
            if (Startup == null)
            {
                throw new Exception("Startup class must be specified");
            }
        }
        public Type Startup { get; set; } = null!;
    }
}
