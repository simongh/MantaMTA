using System.Collections.Generic;
using System.Net;

namespace OpenManta.Data
{
	public interface ICfgRelayingPermittedIP
	{
		IEnumerable<string> GetRelayingPermittedIPAddresses();

		void SetRelayingPermittedIPAddresses(IEnumerable<IPAddress> addresses);
	}
}