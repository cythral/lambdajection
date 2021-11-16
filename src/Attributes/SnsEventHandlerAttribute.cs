using System;
using System.Diagnostics;

using Lambdajection.Framework;

namespace Lambdajection.Attributes
{
    /// <summary>
    /// Class attribute for SNS Event Handlers. Adding this attribute to a class generates a Run method and LambdaConfigurator
    /// subclass. This attribute cannot be accessed using reflection - it is only present at build time.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    [Conditional("CodeGeneration")]
    [LambdaHost("Lambdajection.Sns", "SnsLambdaHost")]
    [LambdaInterface("Lambdajection.Sns", "ISnsEventHandler")]
    public class SnsEventHandlerAttribute : LambdaAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnsEventHandlerAttribute" /> class.
        /// </summary>
        /// <param name="startup">The type of startup class to use for the custom resource provider.</param>
        public SnsEventHandlerAttribute(Type startup)
            : base(startup)
        {
        }
    }
}
