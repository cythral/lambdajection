using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Lambdajection.Attributes;

namespace Lambdajection.CompilationTests.CustomResources
{
    public class ResourceProperties
    {
        [MinLength(3, ErrorMessage = "Expected Error Message")]
        public string Name { get; set; }

        public bool ShouldFail { get; set; } = false;

        // This is a test - the validations generator should not recurse into collection types
        // (it will cause a stack overflow if it does recurse, so simply running it will suffice)
        public List<string> ValidationsGeneratorShouldNotRecurseIntoCollections { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;

        [UpdateRequiresReplacement]
        public ulong Serial { get; set; } = 0UL;

        public Metadata Metadata { get; set; } = new Metadata();
    }
}
