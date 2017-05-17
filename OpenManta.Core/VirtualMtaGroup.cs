﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenManta.Core
{
	/// <summary>
	/// Represents a grouping of IP Address that can be used by the MTA for
	/// sending of messages.
	/// </summary>
	public class VirtualMtaGroup : NamedEntity
	{
		/// <summary>
		/// Collection of the Virtual MTAs that make up this group.
		/// </summary>
		public IList<VirtualMTA> VirtualMtaCollection { get; set; }

		/// <summary>
		/// Timestamp of when this MtaIPGroup instance was created; used for caching.
		/// </summary>
		public DateTimeOffset CreatedTimestamp = DateTimeOffset.UtcNow;

		/// <summary>
		/// Gets a random IP from the collection.
		/// This should be improved to take into account messages sent in last ?.
		/// </summary>
		/// <returns></returns>
		public VirtualMTA GetRandomIP()
		{
			// There are no IP addresses in the group so return null.
			if (VirtualMtaCollection == null)
				return null;

			return VirtualMtaCollection.OrderBy(x => new Random().Next()).FirstOrDefault();
		}

		/// <summary>
		/// Object used for locking in GetIpAddressForSending method.
		/// </summary>
		private static object _SyncLock = new object();

		/// <summary>
		/// Gets an IP Address. Uses <paramref name="mxRecord"/> to load balance accross all IPs in group.
		/// </summary>
		/// <param name="mxRecord">MXRecord of the host wanting to send to.</param>
		/// <returns>MtaIpAddress or NULL if none in group.</returns>
		public VirtualMTA GetVirtualMtasForSending(MXRecord mxRecord)
		{
			lock (_SyncLock)
			{
				string key = mxRecord.Host.ToLowerInvariant();

				// Get the IP address that has sent the least to the mx host.
				VirtualMTA vMTA = VirtualMtaCollection.OrderBy(ipAddr => ipAddr.SendsCounter.GetOrAdd(key, 0)).FirstOrDefault();

				// Get the current sends count.
				int currentSends = 0;
				if (!vMTA.SendsCounter.TryGetValue(key, out currentSends))
					return null;

				// Increment the sends count to include this one.
				vMTA.SendsCounter.AddOrUpdate(key, currentSends + 1, new Func<string, int, int>(delegate (string k, int value) { return value + 1; }));

				// Return the IP Address.
				return vMTA;
			}
		}

		/// <summary>
		/// Constuctor sets defaults.
		/// </summary>
		public VirtualMtaGroup()
		{
			this.VirtualMtaCollection = new List<VirtualMTA>();
		}
	}
}