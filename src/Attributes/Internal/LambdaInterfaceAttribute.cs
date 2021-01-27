using System;

namespace Lambdajection.Attributes
{
    /// <summary>
    /// Build-time only attribute to help determine what interface to apply to a Lambda.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal class LambdaInterfaceAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LambdaInterfaceAttribute" /> class.
        /// </summary>
        /// <param name="assemblyName">The assembly the interface is located in.</param>
        /// <param name="interfaceName">The name of the interface.</param>
        public LambdaInterfaceAttribute(string assemblyName, string interfaceName)
        {
            AssemblyName = assemblyName;
            InterfaceName = interfaceName;
        }

        /// <summary>
        /// Gets the assembly name the lambda parameter delegate assembly.
        /// </summary>
        /// <value>The name of the assembly where the lambda parameter delegate is located.</value>
        public string AssemblyName { get; }

        /// <summary>
        /// Gets the name of the lambda interface.
        /// </summary>
        /// <value>The name of the lambda interface.</value>
        public string InterfaceName { get; }
    }
}
