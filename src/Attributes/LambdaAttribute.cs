using System;
using System.Diagnostics;

using Cythral.CodeGeneration.Roslyn;

namespace Lambdajection.Attributes
{
    /// <summary>
    /// Class attribute for Lambda Handlers. Adding this attribute to a class generates a Run method and LambdaConfigurator
    /// subclass. This attribute cannot be accessed using reflection - it is only present at build time.
    /// </summary>
    [CodeGenerationAttribute("Lambdajection.Generator.LambdaGenerator, Lambdajection.Generator")]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    [Conditional("CodeGeneration")]
    public class LambdaAttribute : Attribute
    {
        /// <value>The type of Startup class to use for the Lambda.  The type passed must implement <c>ILambdaStartup</c>.</value>
        public Type Startup { get; set; } = null!;
    }
}
