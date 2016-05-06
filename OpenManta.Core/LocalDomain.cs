using System;

namespace OpenManta.Core
{
	/// <summary>
	/// Holds a local domain object.
	/// </summary>
	public class LocalDomain : NamedEntity
	{
		/// <summary>
		/// Local domains hostname.
		/// </summary>
		public string Hostname { get; set; }
	}
}

