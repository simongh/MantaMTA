using System;

namespace OpenManta.Core
{
	public class MXRecord
	{
		/// <summary>
		/// The hostname or IP address of a MX
		/// </summary>
		public string Host { get; set; }

		/// <summary>
		/// The preference of the MX
		/// </summary>
		public int Preference { get; set; }

		/// <summary>
		/// The TTL seconds
		/// </summary>
		private uint TTL { get; set; }

		/// <summary>
		/// The date/time that this record was got.
		/// </summary>
		private DateTimeOffset LookupTimestamp { get; set; }

		/// <summary>
		/// Return true if Time To Live has passed. RIP.
		/// </summary>
		public bool Dead
		{
			get
			{
				// If the TTL added to the lookup date is over specified date time then return true.
				return (LookupTimestamp.AddSeconds(this.TTL) < DateTimeOffset.UtcNow);
			}
		}

		/// <summary>
		/// Identifies the source of an MX record.
		/// </summary>
		public MxRecordSrc MxRecordSrc { get; set; }

		public MXRecord(string host, int preference, uint ttl, MxRecordSrc mxRecordSrc)
		{
			Host = host;
			Preference = preference;
			TTL = ttl;
			LookupTimestamp = DateTime.UtcNow;
			MxRecordSrc = mxRecordSrc;
		}
	}
}