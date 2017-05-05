using System.Collections.Generic;

namespace OpenManta.Framework
{
	internal interface IDnsApi
	{
		IEnumerable<string> GetMXRecords(string domain);
	}
}