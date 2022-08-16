using System;

namespace Lambdajection.Framework
{
    /// <summary>
    /// Attribute for annotating assemblies with the lambdajection package version.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public class LambdajectionVersionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LambdajectionVersionAttribute" /> class.
        /// </summary>
        /// <param name="version">Version of the lambdajection package.</param>
        public LambdajectionVersionAttribute(string version)
        {
            Version = version;
        }

        /// <summary>
        /// Gets the version of the Lambdajection package.
        /// </summary>
        public string Version { get; }
    }
}
