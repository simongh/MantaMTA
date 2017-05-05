using OpenManta.Core;

namespace OpenManta.WebLib.DAL
{
	internal interface IOutboundRulesDB
	{
		void Delete(int mxPatternID);

		void Save(OutboundRule outboundRule);

		int Save(OutboundMxPattern mxPattern);
	}
}