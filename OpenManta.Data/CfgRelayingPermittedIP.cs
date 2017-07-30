using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using OpenManta.Core;

namespace OpenManta.Data
{
	//public static class CfgRelayingPermittedIPFactory
	//{
	//	public static ICfgRelayingPermittedIP Instance { get; internal set; }
	//}

	internal class CfgRelayingPermittedIP : ICfgRelayingPermittedIP
	{
		private readonly IMantaDB _mantaDb;

		public CfgRelayingPermittedIP(IMantaDB mantaDb)
		{
			Guard.NotNull(mantaDb, nameof(mantaDb));

			_mantaDb = mantaDb;
		}

		/// <summary>
		/// Gets an array of the IP addresses that are permitted to use this server for relaying from the database.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> GetRelayingPermittedIPAddresses()
		{
			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = @"
SELECT IpAddress
FROM Manta.PermittedRelayIps";
				conn.Open();
				var results = new List<string>();
				SqlDataReader reader = cmd.ExecuteReader();
				while (reader.Read())
					results.Add(reader.GetString("IpAddress"));

				return results;
			}
		}

		/// <summary>
		/// Saves the array of IP Address that are allowed to relay messages through MantaMTA.
		/// Overwrites the existing addresses.
		/// </summary>
		/// <param name="addresses">IP Addresses to allow relaying for.</param>
		public void SetRelayingPermittedIPAddresses(IEnumerable<IPAddress> addresses)
		{
			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = @"DELETE FROM Manta.PermittedRelayIps";
				foreach (IPAddress addr in addresses)
				{
					cmd.CommandText += System.Environment.NewLine + "INSERT INTO Manta.PermittedRelayIps(IpAddress) VALUES ( '" + addr.ToString() + "' )";
				}
				conn.Open();
				cmd.ExecuteNonQuery();
			}
		}
	}
}