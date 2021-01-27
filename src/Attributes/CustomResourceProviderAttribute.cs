using System;
using System.Diagnostics;

namespace Lambdajection.Attributes
{
    /// <summary>
    /// Class attribute for Custom Resource Providers. Adding this attribute to a class generates a Run method and LambdaConfigurator
    /// subclass. This attribute cannot be accessed using reflection - it is only present at build time.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    [Conditional("CodeGeneration")]
    [LambdaHost("Lambdajection.CustomResource", "CustomResourceLambdaHost")]
    [LambdaInterface("Lambdajection.CustomResource", "ICustomResourceProvider")]
    public class CustomResourceProviderAttribute : LambdaAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomResourceProviderAttribute" /> class.
        /// </summary>
        /// <param name="startup">The type of startup class to use for the custom resource provider.</param>
        public CustomResourceProviderAttribute(Type startup)
            : base(startup)
        {
        }
    }
}
