using System;
using System.Diagnostics;

namespace Lambdajection.Attributes
{
    /// <summary>
    /// Class attribute for Lambda Handlers. Adding this attribute to a class generates a Run method and LambdaConfigurator
    /// subclass. This attribute cannot be accessed using reflection - it is only present at build time.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    [Conditional("CodeGeneration")]
    public class LambdaAttribute : Attribute
    {
        /// <summary>
        /// Constructs a new LambdaAttribute.
        /// </summary>
        /// <param name="startup">The type of startup class to use for the lambda.</param>
        public LambdaAttribute(Type startup)
        {
            this.Startup = startup;
        }

        /// <value>The type of Startup class to use for the Lambda.  The type passed must implement <c>ILambdaStartup</c>.</value>
        public Type Startup { get; } = null!;

        /// <value>The type of Serializer to use for the Lambda.</value>
        public Type Serializer { get; set; } = null!;

        /// <value>The type of Config Factory to use for the Lambda.</value>
        public Type ConfigFactory { get; set; } = null!;
    }
}
