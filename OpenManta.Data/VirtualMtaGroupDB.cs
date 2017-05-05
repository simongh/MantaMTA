using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using OpenManta.Core;

namespace OpenManta.Data
{
	public static class VirtualMtaGroupDBFactory
	{
		public static IVirtualMtaGroupDB Instance { get; internal set; }
	}

	internal class VirtualMtaGroupDB : IVirtualMtaGroupDB
	{
		private readonly IDataRetrieval _dataRetrieval;
		private readonly IMantaDB _mantaDb;

		public VirtualMtaGroupDB(IDataRetrieval dataRetrieval, IMantaDB mantaDb)
		{
			Guard.NotNull(dataRetrieval, nameof(dataRetrieval));
			Guard.NotNull(mantaDb, nameof(mantaDb));

			_dataRetrieval = dataRetrieval;
			_mantaDb = mantaDb;
		}

		/// <summary>
		/// Gets a Virtual MTA Group from the database; doesn't include Virtual MTA objects.
		/// </summary>
		/// <param name="ID"></param>
		/// <returns></returns>
		public VirtualMtaGroup GetVirtualMtaGroup(int id)
		{
			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = @"
SELECT *
FROM man_ip_group as [grp]
WHERE [grp].ip_group_id = @groupID";
				cmd.Parameters.AddWithValue("@groupID", id);
				return _dataRetrieval.GetSingleObjectFromDatabase<VirtualMtaGroup>(cmd, CreateAndFillVirtualMtaGroup);
			}
		}

		/// <summary>
		/// Gets all of the Virtual MTA Groups from the database; doesn't include Virtual MTA objects.
		/// </summary>
		/// <returns></returns>
		public IList<VirtualMtaGroup> GetVirtualMtaGroups()
		{
			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = @"
SELECT *
FROM man_ip_group";
				return _dataRetrieval.GetCollectionFromDatabase<VirtualMtaGroup>(cmd, CreateAndFillVirtualMtaGroup);
			}
		}

		/// <summary>
		/// Creates a MtaIPGroup object using the Data Record.
		/// </summary>
		/// <param name="record"></param>
		/// <returns></returns>
		private VirtualMtaGroup CreateAndFillVirtualMtaGroup(IDataRecord record)
		{
			VirtualMtaGroup group = new VirtualMtaGroup();
			group.ID = record.GetInt32("ip_group_id");
			group.Name = record.GetString("ip_group_name");
			if (!record.IsDBNull("ip_group_description"))
				group.Description = record.GetString("ip_group_description");

			return group;
		}

		/// <summary>
		/// Saves the virtual mta group to the database.
		/// </summary>
		/// <param name="grp">Group to save.</param>
		public void Save(VirtualMtaGroup grp)
		{
			Guard.NotNull(grp, nameof(grp));

			StringBuilder groupMembershipInserts = new StringBuilder();
			foreach (VirtualMTA vmta in grp.VirtualMtaCollection)
				groupMembershipInserts.AppendFormat(@"{1}INSERT INTO man_ip_groupMembership(ip_group_id, ip_ipAddress_id)
VALUES(@id,{0}){1}", vmta.ID, Environment.NewLine);

			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = @"
BEGIN TRANSACTION

IF EXISTS(SELECT 1 FROM man_ip_group WHERE ip_group_id = @id)
	UPDATE man_ip_group
	SET ip_group_name = @name,
		ip_group_description = @description
	WHERE ip_group_id = @id
ELSE
	BEGIN
		INSERT INTO man_ip_group(ip_group_name, ip_group_description)
		VALUES(@name, @description)

		SELECT @id = @@IDENTITY
	END

DELETE
FROM man_ip_groupMembership
WHERE ip_group_id = @id

" + groupMembershipInserts.ToString() + @"

COMMIT TRANSACTION";
				cmd.Parameters.AddWithValue("@id", grp.ID);
				cmd.Parameters.AddWithValue("@name", grp.Name);

				if (grp.Description == null)
					cmd.Parameters.AddWithValue("@description", DBNull.Value);
				else
					cmd.Parameters.AddWithValue("@description", grp.Description);
				conn.Open();
				cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Deletes the specified Virtual MTA group.
		/// </summary>
		/// <param name="id">ID of the virtual mta group to delete.</param>
		public void Delete(int id)
		{
			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = @"
DELETE
FROM man_ip_group
WHERE ip_group_id = @id

DELETE
FROM man_ip_groupMembership
WHERE ip_group_id = @id";
				cmd.Parameters.AddWithValue("@id", id);
				conn.Open();
				cmd.ExecuteNonQuery();
			}
		}
	}
}