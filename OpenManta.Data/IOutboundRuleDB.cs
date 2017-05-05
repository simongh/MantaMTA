using System.Collections.Generic;
using OpenManta.Core;

namespace OpenManta.Data
{
	public interface IOutboundRuleDB
	{
		IList<OutboundMxPattern> GetOutboundRulePatterns();

		IList<OutboundRule> GetOutboundRules();
	}
}