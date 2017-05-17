using System;
using System.Linq;
using OpenManta.Core;
using OpenManta.Data;

namespace OpenManta.Framework
{
	internal partial class BounceRulesManager : IBounceRulesManager
	{
		private readonly IEventDB _eventDb;

		public BounceRulesManager(IEventDB eventDb)
		{
			Guard.NotNull(eventDb, nameof(eventDb));

			_eventDb = eventDb;
		}

		private BounceRulesCollection _bounceRules = null;

		public BounceRulesCollection BounceRules
		{
			get
			{
				if (_bounceRules == null || _bounceRules.LoadedTimestampUtc.AddMinutes(5) < DateTimeOffset.UtcNow)
				{
					// Would be nice to write to a log that we're updating.
					_bounceRules = _eventDb.GetBounceRules();

					// Ensure the Rules are in the correct order.
					_bounceRules = new BounceRulesCollection(_bounceRules.OrderBy(r => r.ExecutionOrder));

					// Only set the LoadedTimestamp value after we're done assigning new values to _bounceRules.
					_bounceRules.LoadedTimestampUtc = DateTimeOffset.UtcNow;
				}

				return _bounceRules;
			}
		}
	}
}