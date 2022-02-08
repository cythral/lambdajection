using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using Lambdajection.Framework;

namespace Lambdajection.CustomResource
{
    /// <summary>
    /// Describes a request from CloudFormation to create,
    /// update or delete a Custom Resource.
    /// </summary>
    public class CustomResourceRequest
    {
        /// <summary>
        /// Gets or sets the type of Custom Resource request.
        /// </summary>
        /// <value>The type of Custom Resource request.</value>
        public virtual CustomResourceRequestType RequestType { get; set; }

        /// <summary>
        /// Gets or sets the URL to respond to once the Custom Resource
        /// has been created/updated/deleted.
        /// </summary>
        /// <value>The response URL.</value>
        public virtual Uri? ResponseURL { get; set; }

        /// <summary>
        /// Gets or sets the Id of the CloudFormation stack that the
        /// custom resource will belong to.
        /// </summary>
        /// <value>The ID of the stack the custom resource will belong to.</value>
        public virtual string StackId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ID of this request.
        /// </summary>
        /// <value>The ID of this request.</value>
        public virtual string RequestId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the resource type provided in the CloudFormation template
        /// for the requested custom resource.
        /// </summary>
        /// <value>The resource type provided for the custom resource.</value>
        public virtual string ResourceType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the logical resource ID provided in the CloudFormation template
        /// for the requested custom resource.
        /// </summary>
        /// <value>The logical resource ID of the requested custom resource.</value>
        public virtual string LogicalResourceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the physical resource ID given
        /// for the requested custom resource.
        /// </summary>
        /// <value>The physical resource ID of the requested custom resource.</value>
        public virtual string? PhysicalResourceId { get; set; }

        /// <summary>
        /// Gets or sets the extra properties that appear when deserializing a request to this type.
        /// </summary>
        [JsonExtensionData]
        [NotValidated]
        public virtual IDictionary<string, JsonElement> ExtraProperties { get; set; } = default!;
    }
}
