using System;

namespace OpenManta.Core
{
	/// <summary>
	/// Holds information about an SMTP code returned by a server as a bounce.
	/// </summary>
	public struct BouncePair
	{
		/// <summary>
		/// The MantaBounceCode for the Bounce.
		/// </summary>
		public MantaBounceCode BounceCode;
		/// <summary>
		/// The MentaBounceType for the Bounce.
		/// </summary>
		public MantaBounceType BounceType;

		public override string ToString()
		{
			return String.Format("BounceType: {0}, BounceCode: {1}", this.BounceType, this.BounceCode);
		}
	}
}

