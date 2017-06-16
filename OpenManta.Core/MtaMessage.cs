using System;

namespace OpenManta.Core
{
	/// <summary>
	/// Represents an Inbound Email that is going to be queued for relaying, but has not yet been.
	/// </summary>
	public class MtaMessage : BaseEntity<Guid>
	{
		/// <summary>
		/// ID of the RabbitMQ delivery that this message represents.
		/// </summary>
		public ulong RabbitMqDeliveryTag { get; set; }

		/// <summary>
		/// Priority of this message in RabbitMQ.
		/// </summary>
		public MessagePriority RabbitMqPriority { get; set; }

		/// <summary>
		/// The VirtualMTA group that the message should be sent through.
		/// </summary>
		public int VirtualMTAGroupID { get; set; }

		/// <summary>
		/// Internal ID that identifies the Send that this
		/// message is part of.
		/// </summary>
		public int InternalSendID { get; set; }

		/// <summary>
		/// The Mail From to used when sending this message.
		/// May be NULL for NullSender
		/// </summary>
		public string MailFrom { get; set; }

		/// <summary>
		/// Array of Rcpt To's for this message.
		/// </summary>
		public string[] RcptTo { get; set; }

		/// <summary>
		/// The raw Email itself.
		/// </summary>
		public string Message { get; set; }
	}
}