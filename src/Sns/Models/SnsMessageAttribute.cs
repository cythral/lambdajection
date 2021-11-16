namespace Lambdajection.Sns
{
    /// <summary>
    /// An SNS message attribute.
    /// </summary>
    public class SnsMessageAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnsMessageAttribute" /> class.
        /// </summary>
        /// <param name="type">The attribute's data type.</param>
        /// <param name="value">The attribute's value.</param>
        public SnsMessageAttribute(string type, string value)
        {
            Type = type;
            Value = value;
        }

        /// <summary>
        /// Gets or sets the attribute type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the attribute value.
        /// </summary>
        public string Value { get; set; }
    }
}
