using System;
using System.Diagnostics;

namespace Lambdajection.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    [Conditional("CodeGeneration")]
    public class LambdaOptionsAttribute : Attribute
    {
        public LambdaOptionsAttribute(Type lambdaType, string sectionName)
        {
            this.LambdaType = lambdaType;
            this.SectionName = sectionName;
        }

        public Type LambdaType { get; set; }
        public string SectionName { get; set; }
    }
}
