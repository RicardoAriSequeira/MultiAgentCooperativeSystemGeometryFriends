using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeometryFriends.AI.Communication
{
    /// <summary>
    /// Class that represents an agent message to be tranmitted.
    /// </summary>
    public class AgentMessage
    {
        /// <summary>
        /// The message to be transmitted.
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// The attachment to the message transmitted, if any. WARNING: cross-thread safety is the responsibility of the sender/receiver of the messages.
        /// </summary>
        public object Attachment { get; set; }

        /// <summary>
        /// Constructor for a broadcast agent message.
        /// </summary>
        /// <param name="message">The message to be transmitted.</param>
        /// <param name="attachment">The attachment to the message transmitted, if any. WARNING: cross-thread safety is the responsibility of the sender/receiver of the messages.</param>
        public AgentMessage(string message, object attachment = null)
        {
            Message = message;
            Attachment = attachment;
        }

    }
}
