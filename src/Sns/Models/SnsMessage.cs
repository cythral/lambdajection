using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using Lambdajection.Core.Serialization;

namespace Lambdajection.Sns
{
    /// <summary>
    /// Represents a message received from SNS.
    /// </summary>
    /// <typeparam name="TMessage">The type of message contained within the SNS Event.</typeparam>
    public class SnsMessage<TMessage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnsMessage{TMessage}" /> class.
        /// </summary>
        /// <param name="message">The message body.</param>
        /// <param name="messageAttributes">The message attributes.</param>
        /// <param name="messageId">The message's ID.</param>
        /// <param name="signature">The signature of the message.</param>
        /// <param name="signatureVersion">The version of the message's signature.</param>
        /// <param name="signingCertUrl">The URL of the certificate used to sign the message.</param>
        /// <param name="subject">The subject of the message.</param>
        /// <param name="timestamp">The timestamp of the message.</param>
        /// <param name="topicArn">The topic of the message.</param>
        /// <param name="type">The type of the message.</param>
        /// <param name="unsubscribeUrl">The URL that can be used to unsubscribe from receiving new SNS events.</param>
        public SnsMessage(
            TMessage message,
            Dictionary<string, SnsMessageAttribute> messageAttributes,
            string messageId,
            string signature,
            string signatureVersion,
            Uri signingCertUrl,
            string subject,
            DateTime timestamp,
            string topicArn,
            string type,
            Uri unsubscribeUrl
        )
        {
            Message = message;
            MessageAttributes = messageAttributes;
            MessageId = messageId;
            Signature = signature;
            SignatureVersion = signatureVersion;
            SigningCertUrl = signingCertUrl;
            Subject = subject;
            Timestamp = timestamp;
            TopicArn = topicArn;
            Type = type;
            UnsubscribeUrl = unsubscribeUrl;
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        [JsonConverter(typeof(StringConverterFactory))]
        public TMessage Message { get; set; }

        /// <summary>
        /// Gets or sets the attributes associated with the message.
        /// </summary>
        public Dictionary<string, SnsMessageAttribute> MessageAttributes { get; set; }

        /// <summary>
        /// Gets or sets the message id.
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// Gets or sets the message signature.
        /// </summary>
        public string Signature { get; set; }

        /// <summary>
        /// Gets or sets the signature version used to sign the message.
        /// </summary>
        public string SignatureVersion { get; set; }

        /// <summary>
        /// Gets or sets the URL for the signing certificate.
        /// </summary>
        public Uri SigningCertUrl { get; set; }

        /// <summary>
        /// Gets or sets the subject for the message.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the message time stamp.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the topic ARN.
        /// </summary>
        public string TopicArn { get; set; }

        /// <summary>
        /// Gets or sets the message type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the message unsubscribe URL.
        /// </summary>
        public Uri UnsubscribeUrl { get; set; }
    }
}
