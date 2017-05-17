using System;

namespace OpenManta.Core
{
	public class Send : BaseEntity<string>
	{
		/// <summary>
		/// An Internal ID for the sendID.
		/// </summary>
		public int InternalID { get; set; }

		/// <summary>
		/// The current Status of this Send.
		/// </summary>
		public SendStatus SendStatus { get; set; }

		/// <summary>
		/// This is used to record when this instance of this class was accessed. Used by
		/// the SendIDManager to clean up it's internal cache.
		/// </summary>
		public DateTimeOffset LastAccessedTimestamp { get; set; }

		/// <summary>
		/// Timestamp Send was created.
		/// </summary>
		public DateTimeOffset CreatedTimestamp { get; set; }
	}
}