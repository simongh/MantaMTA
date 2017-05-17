using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using OpenManta.Core;

namespace OpenManta.Data
{
	public static class CfgParaFactory
	{
		public static ICfgPara Instance { get; internal set; }
	}

	internal class CfgPara : ICfgPara
	{
		private readonly IMantaDB _mantaDb;
		private int? _DefaultVirtualMtaGroupID = null;

		public CfgPara(IMantaDB mantaDb)
		{
			Guard.NotNull(mantaDb, nameof(mantaDb));

			_mantaDb = mantaDb;
		}

		/// <summary>
		/// Gets the IP Addresses that SMTP servers should listen for client on from the database.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<int> ServerListenPorts
		{
			get
			{
				string[] results = GetColumnValue("listenPorts").ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				ArrayList toReturn = new ArrayList();
				for (int i = 0; i < results.Length; i++)
					toReturn.Add(Int32.Parse(results[i]));

				return (int[])toReturn.ToArray(typeof(int));
			}
		}

		/// <summary>
		/// Gets the path to the drop folder.
		/// </summary>
		/// <returns></returns>
		public string DropFolder
		{
			get { return GetColumnValue("dropFolder").ToString(); }
		}

		/// <summary>
		/// Gets the path to the queue folder.
		/// </summary>
		/// <returns></returns>
		public string QueueFolder
		{
			get { return GetColumnValue("queueFolder").ToString(); }
		}

		/// <summary>
		/// Gets the path to the log folder.
		/// </summary>
		/// <returns></returns>
		public string LogFolder
		{
			get { return GetColumnValue("logFolder").ToString(); }
		}

		// <summary>
		// Gets or sets the base retry interval used for retry interval calculations.
		// </summary>
		// <returns></returns>
		public int RetryIntervalBaseMinutes
		{
			get { return (int)GetColumnValue("retryIntervalMinutes"); }
			set { SetColumnValue("retryIntervalMinutes", value); }
		}

		/// <summary>
		/// Gets or sets the maximum time a message should be queued for from the DB.
		/// </summary>
		/// <returns></returns>
		public int MaxTimeInQueueMinutes
		{
			get { return (int)GetColumnValue("maxTimeInQueueMinutes"); }
			set { SetColumnValue("maxTimeInQueueMinutes", value); }
		}

		/// <summary>
		/// Gets or sets the ID of the default send group from the DB.
		/// </summary>
		/// <returns></returns>
		public int DefaultVirtualMtaGroupID
		{
			get
			{
				if (_DefaultVirtualMtaGroupID == null)
					_DefaultVirtualMtaGroupID = (int)GetColumnValue("defaultIpGroupId");
				return _DefaultVirtualMtaGroupID.Value;
			}
			set
			{
				SetColumnValue("defaultIpGroupId", value);
			}
		}

		/// <summary>
		/// Gets or sets the client connection idle timeout in seconds from the database.
		/// </summary>
		public int ClientIdleTimeout
		{
			get { return (int)GetColumnValue("clientIdleTimeout"); }
			set { SetColumnValue("clientIdleTimeout", value); }
		}

		/// <summary>
		/// Gets or sets the amount of days to keep SMTP log files from the database.
		/// </summary>
		public int DaysToKeepSmtpLogsFor
		{
			get { return (int)GetColumnValue("maxDaysToKeepSmtpLogs"); }
			set { SetColumnValue("maxDaysToKeepSmtpLogs", value); }
		}

		/// <summary>
		/// Gets the connection receive timeout in seconds from the database.
		/// </summary>
		public int ReceiveTimeout
		{
			get { return (int)GetColumnValue("receiveTimeout"); }
			set { SetColumnValue("receiveTimeout", value); }
		}

		/// <summary>
		/// Gets or sets the return path domain.
		/// </summary>
		public int ReturnPathDomainId
		{
			get
			{
				using (SqlConnection conn = _mantaDb.GetSqlConnection())
				{
					SqlCommand cmd = conn.CreateCommand();
					cmd.CommandText = @"
SELECT [dmn].domain
FROM Manta.LocalDomains as [dmn]
WHERE [dmn].LocalDomainId = (SELECT Value FROM Manta.Settings as [para] WHERE Name = 'returnPathDomain_id')";
					conn.Open();
					return (int)cmd.ExecuteScalar();
				}
			}
			set { SetColumnValue("returnPathDomain_id", value); }
		}

		/// <summary>
		/// Gets or sets the connection send timeout in seconds from the database.
		/// </summary>
		public int SendTimeout
		{
			get { return (int)GetColumnValue("sendTimeout"); }
			set { SetColumnValue("sendTimeout", value); }
		}

		/// <summary>
		/// Gets or sets the URL to post events to from the database.
		/// </summary>
		public string EventForwardingHttpPostUrl
		{
			get { return GetColumnValue("eventForwardingHttpPostUrl").ToString(); }
			set { SetColumnValue("eventForwardingHttpPostUrl", value); }
		}

		/// <summary>
		/// Gets the a flag from the database that indicates whether to keep or delete successfully processed bounce email files.
		/// Useful for Bounce Rule reviewing.
		/// </summary>
		public bool KeepBounceFilesFlag
		{
			get { return (bool)GetColumnValue("keepBounceFilesFlag"); }
		}

		/// <summary>
		/// ExecuteScalar getting value of colName in man_cfg_para
		/// </summary>
		/// <param name="colName"></param>
		/// <returns></returns>
		private object GetColumnValue(string colName)
		{
			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = @"
SELECT Value FROM Manta.Settings WHERE Name=@name";

				cmd.Parameters.AddWithValue("@name", colName);

				conn.Open();
				return cmd.ExecuteScalar();
			}
		}

		/// <summary>
		/// Saves the specified value to the column in the config table.
		/// </summary>
		/// <param name="colName">Name of the column to set.</param>
		/// <param name="value">Value to set.</param>
		private void SetColumnValue(string colName, object value)
		{
			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = @"
UPDATE Manta.Settings
SET value = @value
WHERE Name = @name
";
				conn.Open();
				cmd.Parameters.AddWithValue("@value", value);
				cmd.Parameters.AddWithValue("@name", colName);
				cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Gets the RabbitMQ enabled state from the parameters table.
		/// </summary>
		/// <returns>True if MantaMTA should use RabbitMQ.</returns>
		public bool RabbitMqEnabled
		{
			get { return Convert.ToBoolean(GetColumnValue("rabbitMqEnabled")); }
		}

		/// <summary>
		/// Gets the RabbitMQ username.
		/// </summary>
		/// <returns>RabbitMQ username.</returns>
		public string RabbitMqUsername
		{
			get { return GetColumnValue("rabbitMqUsername").ToString(); }
		}

		/// <summary>
		/// Gets the RabbitMQ password.
		/// </summary>
		/// <returns>RabbitMQ password.</returns>
		public string RabbitMqPassword
		{
			get { return GetColumnValue("rabbitMqPassword").ToString(); }
		}

		/// <summary>
		/// Gets the RabbitMQ hostname.
		/// </summary>
		/// <returns>RabbitMQ hostname.</returns>
		public string RabbitMqHostname
		{
			get { return GetColumnValue("rabbitMqHostname").ToString(); }
		}
	}
}