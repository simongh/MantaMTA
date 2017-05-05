using System.Collections.Generic;
using OpenManta.Core;

namespace OpenManta.Framework
{
	public interface IDnsManager
	{
		IEnumerable<MXRecord> GetMXRecords(string domain);
	}
}