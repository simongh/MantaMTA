using System;

namespace OpenManta.Core
{
	/// <summary>
	/// An Event from Manta indicating an issue sending an email.  Could be a bounce or an abuse complaint.
	/// </summary>
	public class MantaEvent : BaseEntity
	{
		/// <summary>
		/// The type of event that this is.
		/// </summary>
		public MantaEventType EventType { get; set; }

		/// <summary>
		/// The email address that this message was sent to.
		/// </summary>
		public string EmailAddress { get; set; }

		/// <summary>
		/// The identifier for the send.
		/// </summary>
		public string SendID { get; set; }

		/// <summary>
		/// The date and time the event was recorded.
		/// </summary>
		public DateTime EventTime { get; set; }

		/// <summary>
		/// Will be set to true when event has been forwarded.
		/// </summary>
		public bool Forwarded { get; set; }
	}
}

