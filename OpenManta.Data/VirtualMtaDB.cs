using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using OpenManta.Core;

namespace OpenManta.Data
{
	public static class VirtualMtaDBFactory
	{
		public static IVirtualMtaDB Instance { get; internal set; }
	}

	internal class VirtualMtaDB : IVirtualMtaDB
	{
		private readonly IDataRetrieval _dataRetrieval;
		private readonly IMantaDB _mantaDb;

		public VirtualMtaDB(IDataRetrieval dataRetrieval, IMantaDB mantaDb)
		{
			Guard.NotNull(dataRetrieval, nameof(dataRetrieval));
			Guard.NotNull(mantaDb, nameof(mantaDb));

			_dataRetrieval = dataRetrieval;
			_mantaDb = mantaDb;
		}

		/// <summary>
		/// Gets all of the MTA IP Addresses from the Database.
		/// </summary>
		/// <returns></returns>
		public IList<VirtualMTA> GetVirtualMtas()
		{
			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = @"
SELECT *
FROM man_ip_ipAddress";
				return _dataRetrieval.GetCollectionFromDatabase(cmd, CreateAndFillVirtualMtaFromRecord);
			}
		}

		/// <summary>
		/// Gets a single MTA IP Addresses from the Database.
		/// </summary>
		/// <returns></returns>
		public VirtualMTA GetVirtualMta(int id)
		{
			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = @"
SELECT *
FROM man_ip_ipAddress
WHERE ip_ipAddress_id = @id";
				cmd.Parameters.AddWithValue("@id", id);
				return _dataRetrieval.GetSingleObjectFromDatabase(cmd, CreateAndFillVirtualMtaFromRecord);
			}
		}

		/// <summary>
		/// Gets a collection of the Virtual MTAs that belong to a Virtual MTA Group from the database.
		/// </summary>
		/// <param name="groupID">ID of the Virtual MTA Group to get Virtual MTAs for.</param>
		/// <returns></returns>
		public IList<VirtualMTA> GetVirtualMtasInVirtualMtaGroup(int groupID)
		{
			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = @"SELECT *
FROM man_ip_ipAddress as [ip]
WHERE [ip].ip_ipAddress_id IN (SELECT [grp].ip_ipAddress_id FROM man_ip_groupMembership as [grp] WHERE [grp].ip_group_id = @groupID) ";
				cmd.Parameters.AddWithValue("@groupID", groupID);
				return _dataRetrieval.GetCollectionFromDatabase(cmd, CreateAndFillVirtualMtaFromRecord);
			}
		}

		/// <summary>
		/// Creates a VirtualMTA object filled with the values from the DataRecord.
		/// </summary>
		/// <param name="record"></param>
		/// <returns></returns>
		private VirtualMTA CreateAndFillVirtualMtaFromRecord(IDataRecord record)
		{
			VirtualMTA vmta = new VirtualMTA();
			vmta.ID = record.GetInt32("ip_ipAddress_id");
			vmta.Hostname = record.GetString("ip_ipAddress_hostname");
			vmta.IPAddress = System.Net.IPAddress.Parse(record.GetString("ip_ipAddress_ipAddress"));
			vmta.IsSmtpInbound = record.GetBoolean("ip_ipAddress_isInbound");
			vmta.IsSmtpOutbound = record.GetBoolean("ip_ipAddress_isOutbound");
			return vmta;
		}
	}
}