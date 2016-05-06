using System;

namespace OpenManta.Core
{
	/// <summary>
	/// Holds an outbound MX pattern, this is used to match against
	/// an MX servers host name in it's MX record.
	/// </summary>
	public class OutboundMxPattern : NamedEntity
	{
		/// <summary>
		/// The type of this pattern.
		/// </summary>
		public OutboundMxPatternType Type { get; set; }

		/// <summary>
		/// The value to use for matching the MX Record hostname.
		/// </summary>
		public string Value { get; set; }

		/// <summary>
		/// If has value, only apply this pattern against sending from
		/// specified IP address.
		/// </summary>
		public int? LimitedToOutboundIpAddressID { get; set; }

		public OutboundMxPattern()
		{
			// Default to Regex match all.
			this.Type = OutboundMxPatternType.Regex;
			this.Value = ".";
		}
	}
}

