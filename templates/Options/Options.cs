using Lambdajection.Attributes;
using Lambdajection.Encryption;

namespace Project
{
    [LambdaOptions(typeof(Handler), "SectionName")]
    public class Options
    {
#if (encryption)
        [Encrypted]
#endif
        public string Value { get; set; }
    }
}
