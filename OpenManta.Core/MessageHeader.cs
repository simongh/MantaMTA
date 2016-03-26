using System;

namespace OpenManta.Core
{
	/// <summary>
	/// Class represents an Email message header.
	/// </summary>
	public class MessageHeader
	{
		/// <summary>
		/// Name of the message header.
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Message header value.
		/// </summary>
		public string Value { get; set; }

		/// <summary>
		/// Create a new MessageHeader object.
		/// </summary>
		/// <param name="name">Name of the Header.</param>
		/// <param name="value">Headers value.</param>
		public MessageHeader(string name, string value)
		{
			Name = name.Trim();
			Value = value.Trim();
		}
	}
}

