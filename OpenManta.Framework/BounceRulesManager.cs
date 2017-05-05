using System;
using System.Linq;
using OpenManta.Core;
using OpenManta.Data;

namespace OpenManta.Framework
{
	internal partial class BounceRulesManager
	{
		private readonly IEventDB _eventDb;

		/// <summary>
		/// Holds a singleton instance of the BounceRulesManager.
		/// </summary>
		public static BounceRulesManager Instance { get; private set; }

		static BounceRulesManager()
		{
			Instance = new BounceRulesManager(EventDbFactory.Instance);
		}

		private BounceRulesManager(IEventDB eventDb)
		{
			Guard.NotNull(eventDb, nameof(eventDb));

			_eventDb = eventDb;
		}

		private BounceRulesCollection _bounceRules = null;

		public BounceRulesCollection BounceRules
		{
			get
			{
				if (_bounceRules == null || _bounceRules.LoadedTimestampUtc.AddMinutes(5) < DateTime.UtcNow)
				{
					// Would be nice to write to a log that we're updating.
					_bounceRules = _eventDb.GetBounceRules();

					// Ensure the Rules are in the correct order.
					_bounceRules = new BounceRulesCollection(_bounceRules.OrderBy(r => r.ExecutionOrder));

					// Only set the LoadedTimestamp value after we're done assigning new values to _bounceRules.
					_bounceRules.LoadedTimestampUtc = DateTime.UtcNow;
				}

				return _bounceRules;
			}
		}
	}
}