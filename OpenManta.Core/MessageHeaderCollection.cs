using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenManta.Core
{
	/// <summary>
	/// Holds a collection of Email message headers.
	/// </summary>
	public class MessageHeaderCollection : List<MessageHeader>
	{
		public MessageHeaderCollection() { }
		public MessageHeaderCollection(IEnumerable<MessageHeader> collection) : base(collection) { }


		/// <summary>
		/// Retrieves all MessageHeaders found with the Name provided in <paramref name="header"/>.
		/// Search is case-insensitive.
		/// </summary>
		/// <param name="header">The Name of the header to find.  The case of it is not important.</param>
		/// <returns>A MessageHeaderCollection of matches, if any.</returns>
		public MessageHeaderCollection GetAll(string header)
		{
			return new MessageHeaderCollection(this.Where(h => h.Name.Equals(header, StringComparison.OrdinalIgnoreCase)));
		}


		/// <summary>
		/// Retrives the first MessageHeader found with the Name provided in <paramref name="header"/>.
		/// </summary>
		/// <param name="header">The Name of the header to find.  The case of it is not important.</param>
		/// <returns>A MessageHeader object for the first match, else null if no matches were found.</returns>
		public MessageHeader GetFirstOrDefault(string header)
		{
			return this.FirstOrDefault(h => h.Name.Equals(header, StringComparison.OrdinalIgnoreCase));
		}
	}

}

