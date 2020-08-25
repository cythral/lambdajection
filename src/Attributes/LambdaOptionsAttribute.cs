using System;
using System.Diagnostics;

namespace Lambdajection.Attributes
{
    /// <summary>
    /// Attribute to add options to a Lambda.  Adds an IOptions for the targeted class to the specified Lambda. The option values
    /// are taken from the configuration section specified by the <c>SectionName</c> argument.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    [Conditional("CodeGeneration")]
    public class LambdaOptionsAttribute : Attribute
    {
        /// <summary>
        /// Constructs a new LambdaOptionsAttribute.
        /// </summary>
        /// <param name="lambdaType">The lambda type to add options to.</param>
        /// <param name="sectionName">The section name to read option values from.</param>
        public LambdaOptionsAttribute(Type lambdaType, string sectionName)
        {
            this.LambdaType = lambdaType;
            this.SectionName = sectionName;
        }

        /// <value>The lambda type to add options to.</value>
        public Type LambdaType { get; set; }

        /// <value>The section name to read option values from.</value>
        public string SectionName { get; set; }
    }
}
