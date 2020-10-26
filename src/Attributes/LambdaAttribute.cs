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
        /// Initializes a new instance of the <see cref="LambdaAttribute" /> class.
        /// </summary>
        /// <param name="startup">The type of startup class to use for the lambda.</param>
        public LambdaAttribute(Type startup)
        {
            Startup = startup;
        }

        /// <summary>Gets the type of startup class to use for the lambda.</summary>
        /// <value>The type of Startup class to use for the Lambda.  The type passed must implement <c>ILambdaStartup</c>.</value>
        public Type Startup { get; } = null!;

        /// <summary>Gets or sets the serializer to use for the Lambda.</summary>
        /// <value>The type of serializer to use for the Lambda.</value>
        public Type Serializer { get; set; } = null!;

        /// <summary>Gets or sets the Config Factory to use for the Lambda.</summary>
        /// <value>The type of Config Factory to use for the Lambda.</value>
        public Type ConfigFactory { get; set; } = null!;
    }
}
