using System.Collections.Generic;
using System.Data;
using System.Linq;
using OpenManta.Core;

namespace OpenManta.Data
{
	//public static class VirtualMtaDBFactory
	//{
	//	public static IVirtualMtaDB Instance { get; internal set; }
	//}

	internal class VirtualMtaDB : IVirtualMtaDB
	{
		private readonly IMantaDB _mantaDb;

		public VirtualMtaDB(IMantaDB mantaDb)
		{
			Guard.NotNull(mantaDb, nameof(mantaDb));

			_mantaDb = mantaDb;
		}

		/// <summary>
		/// Gets all of the MTA IP Addresses from the Database.
		/// </summary>
		/// <returns></returns>
		public IList<VirtualMTA> GetVirtualMtas()
		{
			return _mantaDb.GetCollectionFromDatabase(@"
SELECT *
FROM Manta.IpAddresses", CreateAndFillVirtualMtaFromRecord).ToList();
		}

		/// <summary>
		/// Gets a single MTA IP Addresses from the Database.
		/// </summary>
		/// <returns></returns>
		public VirtualMTA GetVirtualMta(int id)
		{
			return _mantaDb.GetSingleObjectFromDatabase(@"
SELECT *
FROM Manta.IpAddresses
WHERE IpAddressId = @id", CreateAndFillVirtualMtaFromRecord, cmd => cmd.Parameters.AddWithValue("@id", id));
		}

		/// <summary>
		/// Gets a collection of the Virtual MTAs that belong to a Virtual MTA Group from the database.
		/// </summary>
		/// <param name="groupID">ID of the Virtual MTA Group to get Virtual MTAs for.</param>
		/// <returns></returns>
		public IList<VirtualMTA> GetVirtualMtasInVirtualMtaGroup(int groupID)
		{
			return _mantaDb.GetCollectionFromDatabase(@"
SELECT *
FROM Manta.IpAddresses as [ip]
WHERE [ip].IpAddressId IN (SELECT [grp].IpAddressId FROM Manta.IpGroupMembers as [grp] WHERE [grp].IpGroupId = @groupID)", CreateAndFillVirtualMtaFromRecord, cmd => cmd.Parameters.AddWithValue("@groupID", groupID)).ToList();
		}

		/// <summary>
		/// Creates a VirtualMTA object filled with the values from the DataRecord.
		/// </summary>
		/// <param name="record"></param>
		/// <returns></returns>
		private VirtualMTA CreateAndFillVirtualMtaFromRecord(IDataRecord record)
		{
			VirtualMTA vmta = new VirtualMTA();
			vmta.ID = record.GetInt32("IpAddressId");
			vmta.Hostname = record.GetString("Hostname");
			vmta.IPAddress = System.Net.IPAddress.Parse(record.GetString("IpAddress"));
			vmta.IsSmtpInbound = record.GetBoolean("IsInbound");
			vmta.IsSmtpOutbound = record.GetBoolean("IsOutbound");
			return vmta;
		}
	}
}