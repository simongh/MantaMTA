﻿using System;

namespace OpenManta.Core
{
	/// <summary>
	/// Holds a QUEUED MtaMesage
	/// </summary>
	public class MtaQueuedMessage : MtaMessage
	{
		/// <summary>
		/// Timestamp of the earliest the first/next attempt to send the message should be made.
		/// </summary>
		public DateTimeOffset AttemptSendAfterUtc { get; set; }

		/// <summary>
		/// Number of times that this message has been queued.
		/// </summary>
		public int DeferredCount { get; set; }

		/// <summary>
		///
		/// </summary>
		public bool IsHandled { get; set; }

		/// <summary>
		/// Timestamp of when the message was originally queued.
		/// </summary>
		public DateTimeOffset QueuedTimestampUtc { get; set; }

		/// <summary>
		/// Create a new MtaOutboundMessage from the InboundMessage.
		/// </summary>
		/// <param name="inbound">Inbound message to create this outbound message from.</param>
		/// <returns>The outbound message.</returns>
		public static MtaQueuedMessage CreateNew(MtaMessage inbound)
		{
			return new MtaQueuedMessage
			{
				DeferredCount = 0,
				InternalSendID = inbound.InternalSendID,
				MailFrom = inbound.MailFrom,
				Message = inbound.Message,
				ID = inbound.ID,
				AttemptSendAfterUtc = DateTimeOffset.UtcNow,
				QueuedTimestampUtc = DateTimeOffset.UtcNow,
				RcptTo = inbound.RcptTo,
				VirtualMTAGroupID = inbound.VirtualMTAGroupID,
				IsHandled = false,
				RabbitMqPriority = inbound.RabbitMqPriority
			};
		}
	}
}