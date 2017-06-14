using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using OpenManta.Core;

namespace OpenManta.Data
{
	public static class CfgLocalDomainsFactory
	{
		public static ICfgLocalDomains Instance { get; internal set; }
	}

	internal class CfgLocalDomains : ICfgLocalDomains
	{
		private readonly IMantaDB _mantaDb;

		public CfgLocalDomains(IMantaDB mantaDb)
		{
			Guard.NotNull(mantaDb, nameof(mantaDb));

			_mantaDb = mantaDb;
		}

		/// <summary>
		/// Gets an array of the local domains from the database.
		/// All domains are toLowered!
		/// </summary>
		/// <returns></returns>
		public IList<LocalDomain> GetLocalDomainsArray()
		{
			return _mantaDb.GetCollectionFromDatabase<LocalDomain>(@"
SELECT *
FROM Manta.LocalDomains", CreateAndFillLocalDomainFromRecord).ToList();
		}

		/// <summary>
		/// Deletes all of the local domains from the database.
		/// </summary>
		public void ClearLocalDomains()
		{
			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = @"DELETE FROM Manta.LocalDomains";
				conn.Open();
				cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Saves a local domain to the database.
		/// </summary>
		/// <param name="domain">Domain to add. Does nothing if domain already exists.</param>
		public void Save(LocalDomain localDomain)
		{
			Guard.NotNull(localDomain, nameof(localDomain));

			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = @"
IF EXISTS (SELECT 1 FROM Manta.LocalDomains WHERE LocalDomainId = @id)
	UPDATE Manta.LocalDomains
	SET Domain = @domain,
	Name = @name,
	Description = @description
	WHERE Domain = @id
ELSE
	BEGIN
	IF(@id > 0)
		BEGIN
			SET IDENTITY_INSERT Manta.LocalDomains ON

			INSERT INTO Manta.LocalDomains (LocalDomainId, Domain, Name, Description)
			VALUES(@id, @domain, @name, @description)

			SET IDENTITY_INSERT Manta.LocalDomains OFF
		END
	ELSE
		INSERT INTO Manta.LocalDomains (Domain, Name, Description)
		VALUES(@domain, @name, @description)

	END";
				cmd.Parameters.AddWithValue("@id", localDomain.ID);
				cmd.Parameters.AddWithValue("@domain", localDomain.Hostname);
				cmd.Parameters.AddWithValue("@name", localDomain.Name);

				if (localDomain.Description == null)
					cmd.Parameters.AddWithValue("@description", DBNull.Value);
				else
					cmd.Parameters.AddWithValue("@description", localDomain.Description);
				conn.Open();
				cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Creates a LocalDomain object and fills it with data from the data record.
		/// </summary>
		/// <param name="record">Record to get the data from.</param>
		/// <returns>LocalDomain object filled from record.</returns>
		private LocalDomain CreateAndFillLocalDomainFromRecord(IDataRecord record)
		{
			return new LocalDomain
			{
				Description = record.GetStringOrEmpty("Description"),
				ID = record.GetInt32("LocalDomainId"),
				Name = record.GetStringOrEmpty("Name"),
				Hostname = record.GetString("Domain")
			};
		}
	}
}