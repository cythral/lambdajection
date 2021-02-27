using System;

namespace Lambdajection.Framework
{
    /// <summary>
    /// Build-time only attribute to help determine what lambda host to use
    /// for an attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal class LambdaHostAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LambdaHostAttribute" /> class.
        /// </summary>
        /// <param name="assemblyName">The assembly the host is located in.</param>
        /// <param name="className">The name of the host class.</param>
        public LambdaHostAttribute(string assemblyName, string className)
        {
            AssemblyName = assemblyName;
            ClassName = className;
        }

        /// <summary>
        /// Gets the assembly name the lambda host class is located in.
        /// </summary>
        /// <value>The name of the assembly where the lambda host class is located.</value>
        public string AssemblyName { get; }

        /// <summary>
        /// Gets the name of the lambda host class.
        /// </summary>
        /// <value>The name of the lambda host class.</value>
        public string ClassName { get; }
    }
}
