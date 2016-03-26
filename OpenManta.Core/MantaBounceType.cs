using System;

namespace OpenManta.Core
{
	/// <summary>
	/// Identifies the type of bounce.
	/// </summary>
	public enum MantaBounceType : int
	{
		/// <summary>
		/// Default value.
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// A hard bounce is where the ISP is explicitly saying that this email address is not valid. 
		/// Typically this is a "user does not exist" error message.
		/// </summary>
		Hard = 1,
		/// <summary>
		/// A soft bounce is an error condition, which if it continues, the email address should be considered 
		/// invalid. An example is a "DNS Failure" (MantaBounceCode 21) bounce message. This can happen because the domain 
		/// name no longer exists, or this could happen because the DNS registration expired and will be renewed
		/// tomorrow, or there was a temporary DNS lookup error. If the "DNS failure" messages persist, then we 
		/// know the address is bad.
		/// </summary>
		Soft = 2,
		/// <summary>
		/// The email was rejected as spam. Instead of removing the email addresses from your list, you would
		/// want to solve whatever caused the blocking and restore delivery to these addresses.
		/// </summary>
		Spam = 3
	}
}

