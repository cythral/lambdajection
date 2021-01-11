namespace Lambdajection.Core.CustomResources
{
    /// <summary>
    /// Describes output data concerning a custom resource
    /// and/or its related entities.
    /// </summary>
    public interface ICustomResourceOutputData
    {
        /// <summary>
        /// Gets the ID / PhysicalResourceId associated with the custom resource.
        /// </summary>
        /// <value>The resulting custom resource's physicalResourceId.</value>
        string Id { get; }
    }
}
