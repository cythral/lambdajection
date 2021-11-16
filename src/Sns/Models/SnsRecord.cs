namespace Lambdajection.Sns
{
    /// <summary>
    /// Represents a record within an SNS Event.
    /// </summary>
    /// <typeparam name="TMessage">The type of message contained within the SNS Event.</typeparam>
    public class SnsRecord<TMessage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnsRecord{TMessage}" /> class.
        /// </summary>
        /// <param name="eventSource">The sns record's event source.</param>
        /// <param name="eventSubscriptionArn">The ARN of the record's event subscription.</param>
        /// <param name="eventVersion">The version of the event.</param>
        /// <param name="sns">The sns record's message structure.</param>
        public SnsRecord(
            string eventSource,
            string eventSubscriptionArn,
            string eventVersion,
            SnsMessage<TMessage> sns
        )
        {
            EventSource = eventSource;
            EventSubscriptionArn = eventSubscriptionArn;
            EventVersion = eventVersion;
            Sns = sns;
        }

        /// <summary>
        /// Gets or sets the sns record's event source.
        /// </summary>
        public string EventSource { get; set; }

        /// <summary>
        /// Gets or sets the arn of the sns record's event subscription.
        /// </summary>
        public string EventSubscriptionArn { get; set; }

        /// <summary>
        /// Gets or sets the version of this event.
        /// </summary>
        public string EventVersion { get; set; }

        /// <summary>
        /// Gets or sets the SNS message.
        /// </summary>
        public SnsMessage<TMessage> Sns { get; set; }
    }
}
