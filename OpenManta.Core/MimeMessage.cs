namespace OpenManta.Core
{
	/// <summary>
	/// Mime message class to help handle messages that have been received.
	/// Used by MantaMTA.Core.Events and will be used to display abuse/postmaster@ emails in web interface.
	/// </summary>
	public class MimeMessage
	{
		/// <summary>
		/// Collection of the Mime message headers.
		/// </summary>
		public MessageHeaderCollection Headers { get; set; }

		/// <summary>
		/// Collection of the mime messages body parts.
		/// </summary>
		public BodyPart[] BodyParts { get; set; }
	}
}