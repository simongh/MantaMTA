using System;

namespace OpenManta.Core
{
	/// <summary>
	/// Identifies a Type of outbound rule.
	/// </summary>
	public enum OutboundRuleType : int
	{
		/// <summary>
		/// Rule holds the maximum simultaneous connections value.
		/// </summary>
		MaxConnections = 1,
		/// <summary>
		/// Rule holds the maximum messages per connections value.
		/// </summary>
		MaxMessagesConnection = 2,
		/// <summary>
		/// Rule holds the maximum messages per hour, all connections.
		/// </summary>
		MaxMessagesPerHour = 3
	}
}

