using Lambdajection.Attributes;
using Lambdajection.Encryption;

namespace Lambdajection.Examples.EncryptedOptions
{
    [LambdaOptions(typeof(Handler), "Lambda")]
    public class Options
    {
        [Encrypted]
        public string EncryptedValue { get; set; } = "";

        /// <value>expected plaintext version of the encrypted value</value>
        public string ExpectedValue { get; set; } = "";
    }
}
