using System.Threading;
using System.Threading.Tasks;

using Lambdajection.Framework;

namespace Lambdajection.CustomResource
{
    /// <summary>
    /// Describes a lambdajection-backed custom resource provider.
    /// </summary>
    /// <typeparam name="TResourceProperties">
    /// The type of resource properties that belong to custom resources provided
    /// by this custom resource provider.
    /// </typeparam>
    /// <typeparam name="TResourceOutputData">
    /// The type that describes output data concerning the custom resource
    /// and/or associated entities.
    /// </typeparam>
    public interface ICustomResourceProvider<TResourceProperties, TResourceOutputData>
        where TResourceProperties : class
        where TResourceOutputData : ICustomResourceOutputData
    {
        /// <summary>
        /// Creates a new custom resource with the given properties.
        /// </summary>
        /// <param name="request">The request sent from CloudFormation.</param>
        /// <param name="cancellationToken">Token used to cancel tasks.</param>
        /// <returns>Output data concerning the resource and/or related entities.</returns>
        Task<TResourceOutputData> Create(CustomResourceRequest<TResourceProperties> request, CancellationToken cancellationToken);

        /// <summary>
        /// Updates a custom resource with the given properties.
        /// </summary>
        /// <param name="request">The request sent from CloudFormation.</param>
        /// <param name="cancellationToken">Token used to cancel tasks.</param>
        /// <returns>Output data concerning the resource and/or related entities.</returns>
        Task<TResourceOutputData> Update(CustomResourceRequest<TResourceProperties> request, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a custom resource.
        /// </summary>
        /// <param name="request">The request sent from CloudFormation.</param>
        /// <param name="cancellationToken">Token used to cancel tasks.</param>
        /// <returns>Output data concerning the resource and/or related entities.</returns>
        Task<TResourceOutputData> Delete(CustomResourceRequest<TResourceProperties> request, CancellationToken cancellationToken);

        /// <summary>
        /// Runs validations on the custom resource request.
        /// </summary>
        /// <param name="request">The custom resource request.</param>
        [Generated("Lambdajection.Generator", "Lambdajection.Generator.ValidationsGenerator")]
        void Validate(CustomResourceRequest<TResourceProperties> request);

        /*
        /// <summary>
        /// Determines whether or not the custom resource requires replacement.
        /// </summary>
        /// <param name="request">The custom resource request.</param>
        /// <returns>True if a new resource should be created, or false if the existing resource can be updated.</returns>
        [Generated]
        bool RequiresReplacement(CustomResourceRequest<TResourceProperties> request);*/
    }
}
