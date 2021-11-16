namespace Lambdajection.Sns
{
    /// <summary>
    /// Simple Notification Service Event.
    /// </summary>
    /// <typeparam name="TMessage">The type of message contained within the SNS Event.</typeparam>
    public class SnsEvent<TMessage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnsEvent{TMessage}" /> class.
        /// </summary>
        /// <param name="records">The records that belong to the event.</param>
        public SnsEvent(
            SnsRecord<TMessage>[] records
        )
        {
            Records = records;
        }

        /// <summary>
        /// Gets or sets the records that are a part of this event.
        /// </summary>
        public SnsRecord<TMessage>[] Records { get; set; }
    }
}
