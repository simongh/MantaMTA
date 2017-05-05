using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenManta.Core;

namespace OpenManta.Data
{
	public interface IEventDB
	{
		BounceRulesCollection GetBounceRules();

		MantaEvent GetEvent(int ID);

		IList<MantaEvent> GetEvents();

		IList<MantaEvent> GetEventsForForwarding(int maxEventsToGet);

		Task<int> SaveAsync(MantaEvent evn);
	}
}