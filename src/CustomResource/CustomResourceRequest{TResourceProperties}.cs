using Lambdajection.Framework;

namespace Lambdajection.CustomResource
{
    /// <summary>
    /// Describes a request from CloudFormation to create,
    /// update or delete a Custom Resource.
    /// </summary>
    /// <typeparam name="TResourceProperties">
    /// The type of resource properties that belong to custom resources provided
    /// by this custom resource provider.
    /// </typeparam>
    public class CustomResourceRequest<TResourceProperties> : CustomResourceRequest
    {
        /// <summary>
        /// Gets or sets the resource properties for the requested
        /// custom resource.
        /// </summary>
        /// <value>The resource properties for the requested custom resource.</value>
        public virtual TResourceProperties? ResourceProperties { get; set; }

        /// <summary>
        /// Gets or sets the old resource properties for the requested custom resource.
        /// This will only be present for update requests.
        /// </summary>
        /// <value>The old resource properties for the requested custom resource.</value>
        [NotValidated]
        public virtual TResourceProperties? OldResourceProperties { get; set; }
    }
}
