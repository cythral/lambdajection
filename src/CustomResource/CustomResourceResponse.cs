namespace Lambdajection.CustomResource
{
    /// <summary>
    /// Describes a response to a request to create/update/delete a custom resource.
    /// </summary>
    /// <typeparam name="TResourceOutputData">
    /// The type that describes output data concerning the custom resource
    /// and/or associated entities.
    /// </typeparam>
    public class CustomResourceResponse<TResourceOutputData>
       where TResourceOutputData : ICustomResourceOutputData
    {
        /// <summary>
        /// Gets or sets the status of the custom resource response.
        /// </summary>
        /// <value>The status of the custom resource respnose.</value>
        public CustomResourceResponseStatus Status { get; set; } = CustomResourceResponseStatus.Success;

        /// <summary>
        /// Gets or sets the physical resource ID of the requested custom resource.
        /// </summary>
        /// <value>The physical resource ID of the requested custom resource.</value>
        public string? PhysicalResourceId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the CloudFormation stack the requested custom resource belongs to.
        /// </summary>
        /// <value>The ID of the CloudFormation stack the requested custom resource belongs to.</value>
        public string StackId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the logical resource ID in the CloudFormation stack
        /// that the requested custom resource belongs to.
        /// </summary>
        /// <value>The logical resource ID of the custom resource.</value>
        public string LogicalResourceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ID of the request this response corresponds to.
        /// </summary>
        /// <value>The ID of the request this response corresponds to.</value>
        public string RequestId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the reason for failing (if applicable).
        /// </summary>
        /// <value>The reason for the failed response (if applicable).</value>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the output data for the Custom Resource.
        /// </summary>
        /// <value>The output data for the custom resource.</value>
        public TResourceOutputData? Data { get; set; }
    }
}
