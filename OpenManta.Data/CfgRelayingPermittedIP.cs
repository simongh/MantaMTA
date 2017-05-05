using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using OpenManta.Core;

namespace OpenManta.Data
{
	public static class CfgRelayingPermittedIPFactory
	{
		public static ICfgRelayingPermittedIP Instance { get; internal set; }
	}

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
SELECT cfg_relayingPermittedIp_ip
FROM man_cfg_relayingPermittedIp";
				conn.Open();
				ArrayList results = new ArrayList();
				SqlDataReader reader = cmd.ExecuteReader();
				while (reader.Read())
					results.Add(reader.GetString("cfg_relayingPermittedIp_ip"));

				return (string[])results.ToArray(typeof(string));
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
				cmd.CommandText = @"DELETE FROM man_cfg_relayingPermittedIp";
				foreach (IPAddress addr in addresses)
				{
					cmd.CommandText += System.Environment.NewLine + "INSERT INTO man_cfg_relayingPermittedIp(cfg_relayingPermittedIp_ip) VALUES ( '" + addr.ToString() + "' )";
				}
				conn.Open();
				cmd.ExecuteNonQuery();
			}
		}
	}
}