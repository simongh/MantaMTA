using System;

namespace OpenManta.Core
{
	/// <summary>
	/// Holds an outbound MX pattern, this is used to match against
	/// an MX servers host name in it's MX record.
	/// </summary>
	public class OutboundMxPattern
	{
		/// <summary>
		/// ID of this pattern.
		/// </summary>
		public int ID { get; set; }

		/// <summary>
		/// Name of this pattern.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Description of this pattern.
		/// </summary>
		public string Description { get; set; }

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
			this.ID = 0;
			this.Name = string.Empty;
			this.Description = string.Empty;

			// Default to Regex match all.
			this.Type = OutboundMxPatternType.Regex;
			this.Value = ".";
			this.LimitedToOutboundIpAddressID = null;
		}
	}
}

