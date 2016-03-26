using System;
using System.Collections.Generic;

namespace OpenManta.Core
{
	/// <summary>
	/// Holds a collection of BounceRule objects.
	/// </summary>
	public class BounceRulesCollection : List<BounceRule>
	{
		/// <summary>
		/// When the BounceRules were last loaded into this collection.
		/// If this is "too old", the collection will reload them to ensure configuration changes are used.
		/// </summary>
		public DateTime LoadedTimestampUtc { get; set; }

		/// <summary>
		/// Standard constructor for a BounceRulesCollection.
		/// </summary>
		public BounceRulesCollection() : base() { }

		/// <summary>
		/// Allows copying of a BounceRulesCollection or the creation of one from a collection of Rules,
		/// e.g. a List&lt;BounceRule&gt;.
		/// </summary>
		/// <param name="collection"></param>
		public BounceRulesCollection(IEnumerable<BounceRule> collection) : base(collection) { }
	}
}

