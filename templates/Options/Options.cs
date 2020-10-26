using Lambdajection.Attributes;
#if encryption
using Lambdajection.Encryption;
#endif

namespace __Project__
{
    [LambdaOptions(typeof(Handler), "SectionName")]
    public class __Options__
    {
#if encryption
        [Encrypted]
#endif
        public string Value { get; set; } = "";
    }
}
