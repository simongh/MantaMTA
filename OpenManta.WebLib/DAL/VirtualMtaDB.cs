using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using OpenManta.Core;
using OpenManta.Data;
using OpenManta.WebLib.BO;

namespace OpenManta.WebLib.DAL
{
	internal class VirtualMtaDB : IVirtualMtaDB
	{
		private readonly IMantaDB _mantaDb;

		public VirtualMtaDB(IMantaDB mantaDb)
		{
			Guard.NotNull(mantaDb, nameof(mantaDb));

			_mantaDb = mantaDb;
		}

		/// <summary>
		/// Gets information about VirtualMTA sends for the specified send.
		/// </summary>
		/// <param name="sendID">ID of the send to get information for.</param>
		/// <returns>Information about the usage of each VirtualMTA in the send.</returns>
		public IEnumerable<VirtualMtaSendInfo> GetSendVirtualMTAStats(string sendID)
		{
			return _mantaDb.GetCollectionFromDatabase(@"
--// Get the internal Send ID
DECLARE @internalSendId int
SELECT @internalSendId = [snd].MtaSendId
FROM Manta.MtaSend as [snd] WITH(nolock)
WHERE [snd].mta_send_id = @sndID

DECLARE @usedIpAddressIds table(IpAddressId int)
--// Get the IP addresses used by the send
INSERT INTO @usedIpAddressIds
SELECT DISTINCT(IpAddressId)
FROM Manta.Transactions as [tran] WITH(nolock)
JOIN Manta.MtaSend as [msg] with(nolock) ON [tran].MessageId = [msg].MessageId
WHERE [msg].MtaSendId = @internalSendId

--// Get the actual data
SELECT [ip].*,
	(SELECT COUNT(*) FROM Manta.Transactions as [tran] with(nolock) JOIN Manta.MtaSend as [msg] with(nolock) ON [tran].MessageId = [msg].MessageId WHERE [tran].IpAddressId = [ip].IpAddressId AND [msg].MtaSendId = @internalSendId AND [tran].TransactionStatusId = 4) AS 'Accepted',
	(SELECT COUNT(*) FROM Manta.Transactions as [tran] with(nolock) JOIN Manta.MtaSend as [msg] with(nolock) ON [tran].MessageId = [msg].MessageId WHERE [tran].IpAddressId = [ip].IpAddressId AND [msg].MtaSendId = @internalSendId AND ([tran].TransactionStatusId = 2 OR [tran].TransactionStatusId = 3 OR [tran].TransactionStatusId = 6)) AS 'Rejected',
	(SELECT COUNT(*) FROM Manta.Transactions as [tran] with(nolock) JOIN Manta.MtaSend as [msg] with(nolock) ON [tran].MessageId = [msg].MessageId WHERE [tran].IpAddressId = [ip].IpAddressId AND [msg].MtaSendId = @internalSendId AND [tran].TransactionStatusId = 5) AS 'Throttled',
	(SELECT COUNT(*) FROM Manta.Transactions as [tran] with(nolock) JOIN Manta.MtaSend as [msg] with(nolock) ON [tran].MessageId = [msg].MessageId WHERE [tran].IpAddressId = [ip].IpAddressId AND [msg].MtaSendId = @internalSendId AND [tran].TransactionStatusId = 1) AS 'Deferred'
FROM Manta.IpAddresses as [ip]
WHERE [ip].IpAddressId IN (SELECT * FROM @usedIpAddressIds)", CreateAndFillVirtualMtaSendInfo, cmd => cmd.Parameters.AddWithValue("@sndID", sendID));
		}

		/// <summary>
		/// Creates a VirtualMtaSendInfo object and fills it with data from the data record.
		/// </summary>
		/// <param name="record">Record to get the data from.</param>
		/// <returns>A VirtualMtaSendInfo object filled with data from the data record.</returns>
		public VirtualMtaSendInfo CreateAndFillVirtualMtaSendInfo(IDataRecord record)
		{
			Guard.NotNull(record, nameof(record));

			VirtualMtaSendInfo vinfo = new VirtualMtaSendInfo();

			vinfo.ID = record.GetInt32("IpAddressId");
			vinfo.Hostname = record.GetString("Hostname");
			vinfo.IPAddress = System.Net.IPAddress.Parse(record.GetString("IpAddress"));
			vinfo.IsSmtpInbound = record.GetBoolean("IsInbound");
			vinfo.IsSmtpOutbound = record.GetBoolean("IsOutbound");
			vinfo.Accepted = record.GetInt64("Accepted");
			vinfo.Deferred = record.GetInt64("Deferred");
			vinfo.Rejected = record.GetInt64("Rejected");
			vinfo.Throttled = record.GetInt64("Throttled");

			return vinfo;
		}

		/// <summary>
		/// Save the specified Virtual MTA to the Database.
		/// </summary>
		/// <param name="vmta"></param>
		public void Save(VirtualMTA vmta)
		{
			Guard.NotNull(vmta, nameof(vmta));

			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = @"
IF EXISTS(SELECT 1 FROM Manta.IpAddresses WHERE IpAddressId = @id)
	BEGIN
		UPDATE Manta.IpAddresses
		SET IpAddress = @ipAddress,
			Hostname = @hostname,
			IsInbound = @isInbound,
			IsOutbound = @isOutbound
		WHERE IpAddressId = @id
	END
ELSE
	BEGIN
		INSERT INTO Manta.IpAddresses(IpAddress, Hostname, IsInbound, IsOutbound)
		VALUES(@ipAddress, @hostname, @isInbound, @isOutbound)
	END
";
				cmd.Parameters.AddWithValue("@id", vmta.ID);
				cmd.Parameters.AddWithValue("@ipAddress", vmta.IPAddress.ToString());
				cmd.Parameters.AddWithValue("@hostname", vmta.Hostname);
				cmd.Parameters.AddWithValue("@isInbound", vmta.IsSmtpInbound);
				cmd.Parameters.AddWithValue("@isOutbound", vmta.IsSmtpOutbound);
				conn.Open();
				cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Deletes the specified Virtual MTA from the Database.
		/// </summary>
		/// <param name="id">ID of Virtual MTA to Delete.</param>
		public void Delete(int id)
		{
			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = @"
DELETE FROM Manta.IpAddresses WHERE IpAddressId = @id
DELETE FROM Manta.IpGroupMembers WHERE IpAddressId = @id";
				cmd.Parameters.AddWithValue("@id", id);
				conn.Open();
				cmd.ExecuteNonQuery();
			}
		}
	}
}