using System;

namespace OpenManta.Core
{
	/// <summary>
	/// Manta notification bounce code.
	/// These identify the type of bounce that has occurred.
	/// </summary>
	public enum MantaBounceCode : int
	{
		/// <summary>
		/// Default value.
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// Not actually a bounce.  Gives code processing a bounce a way of finding that it's not actually looking at a bounce, e.g. when finding a 
		/// </summary>
		NotABounce = 1,
		/// <summary>
		/// There is no email account for the email address specified.
		/// </summary>
		BadEmailAddress = 11,
		General = 20,
		DnsFailure = 21,
		MailboxFull = 22,
		MessageSizeTooLarge = 23,
		UnableToConnect = 29,
		ServiceUnavailable = 30,
		/// <summary>
		/// A bounce that we're unable to identify a reason for.
		/// </summary>
		BounceUnknown = 40,
		/// <summary>
		/// Sending server appears on a blocking list.
		/// </summary>
		KnownSpammer = 51,
		/// <summary>
		/// The content of the email has been identified as spam.
		/// </summary>
		SpamDetected = 52,
		AttachmentDetected = 53,
		RelayDenied = 54,
		/// <summary>
		/// Used when a receiving MTA has indicated we're sending too many emails to them.
		/// </summary>
		RateLimitedByReceivingMta = 55,
		/// <summary>
		/// Indicates the receiving server reported an error with the sending address provided by Manta.
		/// </summary>
		ConfigurationErrorWithSendingAddress = 56,
		/// <summary>
		/// The receiving MTA has blocked the IP address.  Contact them to have it removed.
		/// </summary>
		PermanentlyBlockedByReceivingMta = 57,
		/// <summary>
		/// The receiving MTA has placed a temporary block on the IP address, but will automatically remove it after a short period.
		/// </summary>
		TemporarilyBlockedByReceivingMta = 58
	}
}

