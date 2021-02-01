using System;

using Lambdajection.CustomResource;

namespace Lambdajection.Examples.CustomResource
{
    public class Response : ICustomResourceOutputData
    {
        public Response(string password)
        {
            this.Password = password;
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Password { get; set; }
    }
}
