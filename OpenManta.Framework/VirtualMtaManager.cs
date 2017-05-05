using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using OpenManta.Core;
using OpenManta.Data;

namespace OpenManta.Framework
{
	/// <summary>
	/// Manager for VirtualMTAs.
	/// Has own cache (5 min) so the database doesn't need to be hit for every message.
	/// </summary>
	internal class VirtualMtaManager : IVirtualMtaManager
	{
		/// <summary>
		/// Collection of the IP addresses that can be used by the MTA.
		/// </summary>
		private IList<VirtualMTA> _vmtaCollection = null;

		/// <summary>
		/// Collection of the IP addresses that can be used by the MTA for sending.
		/// </summary>
		private IList<VirtualMTA> _outboundMtas = null;

		/// <summary>
		/// Collection of the IP addresses that can be used by the MTA to receive mail.
		/// </summary>
		private IList<VirtualMTA> _inboundMtas = null;

		/// <summary>
		/// Timestamp of when the _ipAddresses collection was filled.
		/// </summary>
		private DateTime _lastGotVirtualMtas = DateTime.MinValue;

		/// <summary>
		/// Collection of cached MtaIPGroupCached.
		/// </summary>
		private ConcurrentDictionary<int, VirtualMtaGroup> _vmtaGroups = new ConcurrentDictionary<int, VirtualMtaGroup>();

		private VirtualMtaGroup _DefaultVirtualMtaGroup;

		/// <summary>
		/// Object used to lock inside the GetMtaIPGroup method.
		/// </summary>
		private static object _MtaVirtualMtaGroupSyncLock = new object();

		private readonly ICfgPara _config;
		private readonly IVirtualMtaDB _virtualMtaDb;
		private readonly IVirtualMtaGroupDB _virtualMtaGroupDb;

		public VirtualMtaManager(ICfgPara config, IVirtualMtaDB virtualMtaDb, IVirtualMtaGroupDB virtualMtaGroupDb)
		{
			Guard.NotNull(config, nameof(config));
			Guard.NotNull(virtualMtaDb, nameof(virtualMtaDb));
			Guard.NotNull(virtualMtaGroupDb, nameof(virtualMtaGroupDb));

			_config = config;
			_virtualMtaDb = virtualMtaDb;
			_virtualMtaGroupDb = virtualMtaGroupDb;
		}

		/// <summary>
		/// Method will load IP addresses from the database if required.
		/// This method should be called before doing anything with the
		/// private IP collections.
		/// </summary>
		private void LoadVirtualMtas()
		{
			if (_vmtaCollection != null &&
				_lastGotVirtualMtas.AddMinutes(MtaParameters.MTA_CACHE_MINUTES) > DateTime.UtcNow)
				return;

			_outboundMtas = null;
			_inboundMtas = null;
			_vmtaCollection = _virtualMtaDb.GetVirtualMtas();
		}

		/// <summary>
		/// Returns a collection of IP addresses that should be
		/// used by the MTA for receiving messages.
		/// </summary>
		/// <returns></returns>
		public IList<VirtualMTA> GetVirtualMtasForListeningOn()
		{
			LoadVirtualMtas();

			if (_inboundMtas == null)
				_inboundMtas = (from ip
								in _vmtaCollection
								where ip.IsSmtpInbound
								select ip).ToList();

			return _inboundMtas;
		}

		/// <summary>
		/// Gets a collection of IP address that can be used
		/// by the MTA for sending of messages.
		/// </summary>
		/// <returns></returns>
		public IList<VirtualMTA> GetVirtualMtasForSending()
		{
			LoadVirtualMtas();

			if (_outboundMtas == null)
				_outboundMtas = (from ip
								 in _vmtaCollection
								 where ip.IsSmtpOutbound
								 select ip).ToList();

			return _outboundMtas;
		}

		/// <summary>
		/// Gets the default MTA IP Group.
		/// </summary>
		/// <returns></returns>
		public VirtualMtaGroup GetDefaultVirtualMtaGroup()
		{
			if (_DefaultVirtualMtaGroup == null)
			{
				int defaultGroupID = _config.DefaultVirtualMtaGroupID;
				_DefaultVirtualMtaGroup = GetVirtualMtaGroup(defaultGroupID);
			}

			return _DefaultVirtualMtaGroup;
		}

		/// <summary>
		/// Gets the specfied MTA IP Group
		/// </summary>
		/// <param name="id">ID of the group to get.</param>
		/// <returns>The IP Group or NULL if doesn't exist.</returns>
		public VirtualMtaGroup GetVirtualMtaGroup(int id)
		{
			VirtualMtaGroup group = null;

			// Try and get IPGroup from the in memory collection.
			if (_vmtaGroups.TryGetValue(id, out group))
			{
				// Only cache IP Groups for N minutes.
				if (group.CreatedTimestamp.AddMinutes(MtaParameters.MTA_CACHE_MINUTES) > DateTime.UtcNow)
					return group;
			}

			// We need to goto the database to get the group. Lock!
			lock (_MtaVirtualMtaGroupSyncLock)
			{
				// Check that something else didn't already load from the database.
				// If it did then we can just return that.
				_vmtaGroups.TryGetValue(id, out group);
				if (group != null && group.CreatedTimestamp.AddMinutes(MtaParameters.MTA_CACHE_MINUTES) > DateTime.UtcNow)
					return group;

				// Get group from the database.
				group = _virtualMtaGroupDb.GetVirtualMtaGroup(id);

				// Group doesn't exist, so don't try and get it's IPs
				if (group == null)
					return null;

				// Got the group, go get it's IPs.
				group.VirtualMtaCollection = _virtualMtaDb.GetVirtualMtasInVirtualMtaGroup(id);

				// Add the group to collection, so others can use it.
				_vmtaGroups.AddOrUpdate(id, group, new Func<int, VirtualMtaGroup, VirtualMtaGroup>(delegate (int key, VirtualMtaGroup existing)
				{
					return group;
				}));
				return group;
			}
		}
	}
}