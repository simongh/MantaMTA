using OpenManta.Core;

namespace OpenManta.WebLib
{
	public interface IOutboundRuleWebManager
	{
		int CreatePattern(string name, string description, OutboundMxPatternType type, string pattern, int? ipAddress);

		void Delete(int mxPatternID);

		int Save(OutboundMxPattern pattern);

		void Save(OutboundRule rule);
	}
}