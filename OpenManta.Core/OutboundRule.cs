namespace OpenManta.Core
{
	/// <summary>
	/// Holds a rule for outbound clients.
	/// </summary>
	public class OutboundRule
	{
		/// <summary>
		/// ID of the pattern that this rule should be used with.
		/// </summary>
		public int OutboundMxPatternID { get; set; }

		/// <summary>
		/// Identifies the type of this rule.
		/// </summary>
		public OutboundRuleType Type { get; set; }

		/// <summary>
		/// The value of this rule.
		/// </summary>
		public string Value { get; set; }

		public OutboundRule(int outbounbMxPatternID, OutboundRuleType type, string value)
		{
			OutboundMxPatternID = outbounbMxPatternID;
			Type = type;
			Value = value;
		}
	}
}