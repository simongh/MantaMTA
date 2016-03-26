using System;

namespace OpenManta.Core
{
	/// <summary>
	/// Manta Bounce Event Notification.
	/// </summary>
	public class MantaBounceEvent : MantaEvent
	{
		/// <summary>
		/// The type of bounce.
		/// </summary>
		public BouncePair BounceInfo;
		/// <summary>
		/// The text of the failure message. (Up to the number of characters configured.)
		/// </summary>
		public string Message { get; set; }
	}

}

