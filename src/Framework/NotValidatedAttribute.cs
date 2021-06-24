using System;

namespace Lambdajection.Framework
{
    /// <summary>
    /// Attribute to annotate properties that are not validated.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NotValidatedAttribute : Attribute
    {
    }
}
