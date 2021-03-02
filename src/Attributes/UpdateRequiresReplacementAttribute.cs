using System;
using System.Diagnostics;

namespace Lambdajection.Attributes
{
    /// <summary>
    /// Attribute used to denote properties that, when changed, cause a custom resource to be replaced.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    [Conditional("CodeGeneration")]
    public class UpdateRequiresReplacementAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateRequiresReplacementAttribute" /> class.
        /// </summary>
        public UpdateRequiresReplacementAttribute()
        {
        }
    }
}
