using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using OpenManta.Core;

namespace OpenManta.Framework
{
	[Serializable]
	internal class DNSDomainNotFoundException : Exception { }

	internal class DNSManager : IDnsManager
	{
		/// <summary>
		/// Holds a thread safe collection of MX Records so we don't need to query the DNS API every time.
		/// </summary>
		private ConcurrentDictionary<string, MXRecord[]> _Records;

		private readonly IDnsApi _dnsApi;

		public DNSManager(IDnsApi dnsApi)
		{
			Guard.NotNull(dnsApi, nameof(dnsApi));

			_dnsApi = dnsApi;
			_Records = new ConcurrentDictionary<string, MXRecord[]>();
		}

		/// <summary>
		/// Gets an Array of MX Records for the specified domain. If none found returns null.
		/// </summary>
		/// <param name="domain"></param>
		/// <returns></returns>
		public IEnumerable<MXRecord> GetMXRecords(string domain)
		{
			// Make sure the domain is all lower.
			domain = domain.ToLower();

			// This is what we'll be returning.
			MXRecord[] mxRecords = null;

			// Try and get DNS from internal cache.
			if (_Records.TryGetValue(domain, out mxRecords))
			{
				// Found cached records.
				// Make sure they haven't expired.
				if (mxRecords.Count((MXRecord mx) => mx.Dead) < 1)
					return mxRecords;
			}

			IEnumerable<string> records = null;

			try
			{
				// Get the records from DNS
				records = _dnsApi.GetMXRecords(domain);
			}
			catch (DNSDomainNotFoundException)
			{
				// Ensure records is null.
				records = null;
			}

			// No MX records for domain.
			if (records == null)
			{
				// If there are no MX records use the hostname as per SMTP RFC.
				MXRecord[] mxs = new MXRecord[]
				{
					new MXRecord(domain, 10, 300u, MxRecordSrc.A)
				};
				_Records.AddOrUpdate(domain, mxs, (string key, MXRecord[] existing) => mxs);
				return mxs;
			}

			mxRecords = new MXRecord[records.Count()];
			for (int i = 0; i < mxRecords.Length; i++)
			{
				string[] split = records.ElementAt(i).Split(new char[] { ',' });
				if (split.Length == 3)
					mxRecords[i] = new MXRecord(split[1], int.Parse(split[0]), uint.Parse(split[2]), MxRecordSrc.MX);
			}

			// Order by preferance
			mxRecords = (
				from mx in mxRecords
				where mx != null
				orderby mx.Preference
				select mx).ToArray<MXRecord>();
			_Records.TryAdd(domain, mxRecords);
			return mxRecords;
		}
	}
}