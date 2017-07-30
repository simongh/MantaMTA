using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using OpenManta.Core;

namespace OpenManta.Data
{
	//public static class MtaTransactionFactory
	//{
	//	public static IMtaTransaction Instance { get; internal set; }
	//}

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
FROM Manta.Transactions WITH(readuncommitted)
WHERE MessageId = @msgID
AND TransactionStatusId IN (2,3,4,6))
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
INSERT INTO Manta.Transactions (MessageId, IpAddressId, CreatedAt, TransactionStatusId, ServerResponse, ServerHostname)
VALUES(@msgID, @ipAddressID, GETUTCDATE(), @status, @serverResponse, @serverHostname)";

				switch (status)
				{
					case TransactionStatus.Discarded:
					case TransactionStatus.Failed:
					case TransactionStatus.TimedOut:
						cmd.CommandText += @"UPDATE Manta.MtaSend
								SET Rejected = Rejected + 1
								WHERE MtaSendId = @sendInternalID";
						break;

					case TransactionStatus.Success:
						cmd.CommandText += @"UPDATE Manta.MtaSend
								SET Accepted = Accepted + 1
								WHERE MtaSendId = @sendInternalID";
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