using System;

using Lambdajection.CustomResource;

namespace Lambdajection.Examples.SnsHandler
{
    public class Response : ICustomResourceOutputData
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
    }
}
