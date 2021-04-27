using System;

namespace Lambdajection.Framework
{
    /// <summary>
    /// An attribute to annotate methods with required iam permissions needed for successfully invoking the method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RequiresIamPermissionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequiresIamPermissionAttribute" /> class.
        /// </summary>
        /// <param name="permission">The required IAM permission for the call.</param>
        public RequiresIamPermissionAttribute(string permission)
        {
            Permission = permission;
        }

        /// <summary>
        /// Gets the permission required for this call.
        /// </summary>
        public string Permission { get; }
    }
}
