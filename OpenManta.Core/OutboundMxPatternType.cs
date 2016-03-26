using System;

namespace OpenManta.Core
{
	/// <summary>
	/// Identifies the type of pattern to match with.
	/// </summary>
	public enum OutboundMxPatternType : int
	{
		/// <summary>
		/// Value is a regular expression.
		/// </summary>
		Regex = 1,
		/// <summary>
		/// Value is a comma delimited list of string to equals.
		/// </summary>
		CommaDelimited = 2
	}
}

