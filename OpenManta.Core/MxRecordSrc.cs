using System;

namespace OpenManta.Core
{
	/// <summary>
	/// Identifies where the MX record came from.
	/// </summary>
	public enum MxRecordSrc
	{
		Unknown = 0,
		/// <summary>
		/// MX record exists in DNS.
		/// </summary>
		MX = 1,
		/// <summary>
		/// No MX record in DNS, using A instead.
		/// </summary>
		A = 2
	}
}

