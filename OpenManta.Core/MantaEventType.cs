using System;

namespace OpenManta.Core
{
	/// <summary>
	/// This identifies the type of an event.
	/// </summary>
	public enum MantaEventType : int
	{
		/// <summary>
		/// Event of a Type unknown
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// Event occurs when delivery of a message is unsuccessful.
		/// </summary>
		Bounce = 1,
		/// <summary>
		/// This event occurs so that you can remove the address that generated the complaint from your database or 
		/// stop sending to it. You can also use this event to maintain statistical information on the number of spam 
		/// complaints created by each campaign. Continuing to send to an address that has complained about spam can 
		/// have a negative effect on your deliverability.
		/// </summary>
		Abuse = 3,
		/// <summary>
		/// Event occurs when a message has been in Manta's outbound queue over the <paramref name="MantaMTA.Core.MtaParameters.MtaMaxTimeInQueue"/> value
		/// without it being successfully accepted by a remote MTA. Manta stops attempting to send it and creates a TimedOutInQueue Event.
		/// </summary>
		TimedOutInQueue = 4
	}

}

