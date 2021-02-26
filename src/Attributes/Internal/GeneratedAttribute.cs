using System;
using System.Diagnostics;

namespace Lambdajection.Attributes
{
    /// <summary>
    /// Attribute used to denote methods that are generated by the Lambdajection Generator.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    [Conditional("CodeGeneration")]
    internal class GeneratedAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratedAttribute" /> class.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly where to find the generator for generating the attributed method.</param>
        /// <param name="typeName">Name of the type where to find the generator for generating the attributed method.</param>
        public GeneratedAttribute(string assemblyName, string typeName)
        {
            AssemblyName = assemblyName;
            TypeName = typeName;
        }

        /// <summary>
        /// Gets or sets the name of the assembly where to find the generator for generating the attributed method.
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// Gets or sets the name of the type of generator to use for generating the attributed method.
        /// </summary>
        public string TypeName { get; set; }
    }
}
