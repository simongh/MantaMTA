using System.Collections.Generic;

namespace OpenManta.WebLib.BO
{
	public class SendMetadata
	{
		public string Name { get; set; }
		public string Value { get; set; }
	}

	public class SendMetadataCollection : List<SendMetadata>
	{
		public SendMetadataCollection()
		{
		}

		public SendMetadataCollection(IEnumerable<SendMetadata> collection) : base(collection)
		{
		}
	}
}