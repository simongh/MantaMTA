using OpenManta.Core;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace OpenManta.Data
{
	public static class MtaTransactionFactory
	{
		public static IMtaTransaction Instance { get; internal set; }
	}

	internal class MtaTransaction : IMtaTransaction
	{
		private readonly IMantaDB _mantaDb;

		public MtaTransaction(IMantaDB mantaDb)
		{
			Guard.NotNull(mantaDb, nameof(mantaDb));

			_mantaDb = mantaDb;
		}

		public async Task<bool> HasBeenHandledAsync(Guid messageID)
		{
			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = @"IF EXISTS(SELECT 1
FROM man_mta_transaction WITH(readuncommitted)
WHERE man_mta_transaction.mta_msg_id = @msgID
AND man_mta_transaction.mta_transactionStatus_id IN (2,3,4,6))
	SELECT 1
ELSE
	SELECT 0";
				cmd.Parameters.AddWithValue("@msgID", messageID);
				await conn.OpenAsync();
				return Convert.ToBoolean(await cmd.ExecuteScalarAsync());
			}
		}

		/// <summary>
		/// Logs an MTA Transaction to the database.
		/// </summary>
		public void LogTransaction(MtaMessage msg, TransactionStatus status, string svrResponse, VirtualMTA ipAddress, MXRecord mxRecord)
		{
			LogTransactionAsync(msg, status, svrResponse, ipAddress, mxRecord).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Logs an MTA Transaction to the database.
		/// </summary>
		public async Task<bool> LogTransactionAsync(MtaMessage msg, TransactionStatus status, string svrResponse, VirtualMTA ipAddress, MXRecord mxRecord)
		{
			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = @"
BEGIN TRANSACTION
INSERT INTO man_mta_transaction (mta_msg_id, ip_ipAddress_id, mta_transaction_timestamp, mta_transactionStatus_id, mta_transaction_serverResponse, mta_transaction_serverHostname)
VALUES(@msgID, @ipAddressID, GETUTCDATE(), @status, @serverResponse, @serverHostname)";

				switch (status)
				{
					case TransactionStatus.Discarded:
					case TransactionStatus.Failed:
					case TransactionStatus.TimedOut:
						cmd.CommandText += @"UPDATE man_mta_send
								SET mta_send_rejected = mta_send_rejected + 1
								WHERE mta_send_internalID = @sendInternalID";
						break;

					case TransactionStatus.Success:
						cmd.CommandText += @"UPDATE man_mta_send
								SET mta_send_accepted = mta_send_accepted + 1
								WHERE mta_send_internalID = @sendInternalID";
						break;
				}

				cmd.CommandText += " COMMIT TRANSACTION";
				cmd.Parameters.AddWithValue("@sendInternalID", msg.InternalSendID);

				cmd.Parameters.AddWithValue("@msgID", msg.ID);
				if (ipAddress != null)
					cmd.Parameters.AddWithValue("@ipAddressID", ipAddress.ID);
				else
					cmd.Parameters.AddWithValue("@ipAddressID", DBNull.Value);

				if (mxRecord != null)
					cmd.Parameters.AddWithValue("@serverHostname", mxRecord.Host);
				else
					cmd.Parameters.AddWithValue("@serverHostname", DBNull.Value);

				cmd.Parameters.AddWithValue("@status", (int)status);
				cmd.Parameters.AddWithValue("@serverResponse", svrResponse);
				await conn.OpenAsync();
				await cmd.ExecuteNonQueryAsync();
				return true;
			}
		}
	}
}