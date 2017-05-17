using System;
using System.Collections.Concurrent;

namespace OpenManta.Framework
{
	/// <summary>
	/// 421 Service not available likley means that an IP is being blocked.
	/// This manager helps us by keeping tack of these and we can use it to
	/// only send a maximum of 1 message/minute from IPs that seem to be blocked.
	/// </summary>
	internal static class ServiceNotAvailableManager
	{
		/// <summary>
		/// Dictionary<IPAddress, Dictionary<Hostname, LastUnavailable>
		/// </summary>
		public static ConcurrentDictionary<string, ConcurrentDictionary<string, DateTimeOffset>> _ServiceUnavailableLog = new ConcurrentDictionary<string, ConcurrentDictionary<string, DateTimeOffset>>();

		/// <summary>
		/// Add a service unavailable event.
		/// </summary>
		/// <param name="ip"></param>
		/// <param name="mxHostname"></param>
		/// <param name="lastAvailable"></param>
		public static void Add(string ip, string mxHostname, DateTimeOffset lastFail)
		{
			mxHostname = mxHostname.ToLower();
			_ServiceUnavailableLog.TryAdd(ip, new ConcurrentDictionary<string, DateTimeOffset>());
			ConcurrentDictionary<string, DateTimeOffset> ipServices = _ServiceUnavailableLog[ip];
			ipServices.AddOrUpdate(mxHostname, lastFail, delegate (string key, DateTimeOffset existingValue)
			{
				// We should only use the "new" timestamp if it's a later date that the existing value.
				// If the existing value is "newer" then a different thread updated already with better data.
				if (existingValue > lastFail)
					return existingValue;
				return lastFail;
			});
		}

		private static readonly TimeSpan ServiceUnavailableTimeSpan = new TimeSpan(0, 0, 30);

		/// <summary>
		/// Check to see if the MX hostname has denied the specified IP access within the last 1 minute.
		/// </summary>
		/// <param name="ip">IP to check</param>
		/// <param name="mxHostname">Hostname of the MX to check</param>
		/// <returns>TRUE if service is unavailable</returns>
		public static bool IsServiceUnavailable(string ip, string mxHostname)
		{
			mxHostname = mxHostname.ToLower();
			ConcurrentDictionary<string, DateTimeOffset> ipServices = null;
			if (_ServiceUnavailableLog.TryGetValue(ip, out ipServices))
			{
				DateTimeOffset lastFail = DateTimeOffset.MinValue;

				if (ipServices.TryGetValue(mxHostname, out lastFail))
				{
					if ((DateTimeOffset.UtcNow - lastFail) < ServiceUnavailableTimeSpan)
						return true;
				}
			}

			return false;
		}
	}
}