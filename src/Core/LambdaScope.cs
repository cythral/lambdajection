using Amazon.Lambda.Core;

#pragma warning disable SA1600, CS1591

namespace Lambdajection.Core
{
    /// <summary>
    /// Container for lambda invocation-specific variables.
    /// </summary>
    public class LambdaScope
    {
        /// <summary>
        /// Gets or sets the lambda context.
        /// </summary>
        /// <value>The lambda context.</value>
        public virtual ILambdaContext? LambdaContext { get; set; }
    }
}
