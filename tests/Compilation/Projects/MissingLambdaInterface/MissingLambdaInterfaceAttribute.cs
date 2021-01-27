using System;
using System.Diagnostics;

using Lambdajection.Attributes;

namespace Lambdajection.CompilationTests.MissingLambdaInterface
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    [Conditional("CodeGeneration")]
    [LambdaHost("Lambdajection.Core", "DefaultLambdaHost")]
    [LambdaInterface("Lambdajection.Core", "NonExistentInterface")]
    public class MissingLambdaInterfaceAttribute : LambdaAttribute
    {
        public MissingLambdaInterfaceAttribute(Type startup)
            : base(startup)
        {
        }
    }
}
