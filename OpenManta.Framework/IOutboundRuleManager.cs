using System.Collections.Generic;
using OpenManta.Core;

namespace OpenManta.Framework
{
	internal interface IOutboundRuleManager
	{
		IList<OutboundRule> GetRules(MXRecord mxRecord, VirtualMTA mtaIpAddress, out int mxPatternID);

		int GetMaxMessagesPerConnection(MXRecord record, VirtualMTA ipAddress);

		int GetMaxMessagesDestinationHour(VirtualMTA vmta, MXRecord mx);

		int GetMaxMessagesDestinationHour(VirtualMTA ipAddress, MXRecord record, out int mxPatternID);

		int GetMaxConnectionsToDestination(VirtualMTA ipAddress, MXRecord record);
	}
}