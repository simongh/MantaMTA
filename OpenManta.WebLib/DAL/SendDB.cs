using OpenManta.Core;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using OpenManta.WebLib.BO;
using OpenManta.Data;

namespace OpenManta.WebLib.DAL
{
	internal class SendDB : ISendDB
	{
		private readonly IMantaDB _mantaDb;

		public SendDB(IMantaDB mantaDb)
		{
			Guard.NotNull(mantaDb, nameof(mantaDb));

			_mantaDb = mantaDb;
		}

		/// <summary>
		/// Gets the amount of messages currently the queue with the specified statuses.
		/// </summary>
		/// <returns>Count of the messages waiting in the queue.</returns>
		public long GetQueueCount(SendStatus[] sendStatus)
		{
			return 0;
			/*using (SqlConnection conn = MantaDB.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = @"SELECT SUM(s.mta_send_messages) - SUM(s.mta_send_accepted + s.mta_send_rejected)
FROM man_mta_send as s WITH(nolock)
WHERE s.mta_sendStatus_id in (" + string.Join(",", Array.ConvertAll<SendStatus, int>(sendStatus, s => (int)s)) + ")";
				conn.Open();
				Int64 result = Convert.ToInt64(cmd.ExecuteScalar());
				if (result < 0)
					return 0;
				return result;
			}*/
		}

		/// <summary>
		/// Get a count of all the sends in the MantaMTA database.
		/// </summary>
		/// <returns>Count of all Sends.</returns>
		public long GetSendsCount()
		{
			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = @"SELECT COUNT(*) FROM Manta.MtaSend";
				conn.Open();
				return Convert.ToInt64(cmd.ExecuteScalar());
			}
		}

		/// <summary>
		/// Gets a page sends information.
		/// </summary>
		/// <param name="pageSize">Size of the page to get.</param>
		/// <param name="pageNum">The page to get.</param>
		/// <returns>SendInfoCollection of the data page.</returns>
		public SendInfoCollection GetSends(int pageSize, int pageNum)
		{
			var results = _mantaDb.GetCollectionFromDatabase(@"
DECLARE @sends table (RowNum int, mta_send_internalId int)

INSERT INTO @sends
SELECT [sends].RowNumber, [sends].mta_send_internalId
FROM (SELECT (ROW_NUMBER() OVER(ORDER BY CreatedAt DESC)) as RowNumber, MtaSendId
FROM Manta.MtaSend with(nolock)) [sends]
WHERE [sends].RowNumber >= " + ((pageNum * pageSize) - pageSize + 1) + " AND [sends].RowNumber <= " + (pageSize * pageNum) + @"

SELECT [send].*,
	Messages,
	Accepted,
	Rejected,
	([send].Messages - (Accepted + Rejected)) AS 'Waiting',
	(SELECT COUNT(*) FROM Manta.Transactions as [tran] with(nolock) JOIN Manta.Messages as [msg] ON [tran].MessageId = [msg].MessageId WHERE [msg].MtaSendId = [send].MtaSendId AND [tran].TransactionStatusId = 5) AS 'Throttled',
	(SELECT COUNT(*) FROM Manta.Transactions as [tran] with(nolock) JOIN Manta.Messages as [msg] ON [tran].MessageId = [msg].MessageId WHERE [msg].MtaSendId = [send].MtaSendId AND [tran].TransactionStatusId = 1) AS 'Deferred',
	(SELECT MAX(CreatedAt) FROM Manta.Transactions as [tran] with(nolock) JOIN  Manta.Messages as [msg] ON [tran].MessageId = [msg].MessageId WHERE [msg].MtaSendId = [send].MtaSendId) AS 'LastTransactionTimestamp'
FROM Manta.MtaSend as [send] with(nolock)
WHERE [send].MtaSendId in (SELECT [s].MtaSendId FROM @sends as [s])
ORDER BY [send].CreatedAt DESC", CreateAndFillSendInfo, cmd =>
			{
				cmd.CommandTimeout = 90;
			});

			return new SendInfoCollection(results);
		}

		/// <summary>
		/// Gets all of the sends with messages waiting to be sent.
		/// </summary>
		/// <returns>SendInfoCollection of all relevent sends.</returns>
		public SendInfoCollection GetSendsInProgress()
		{
			var results = _mantaDb.GetCollectionFromDatabase(@"
SELECT [send].*,
	Messages,
	Accepted,
	Rejected,
	([send].Messages - (Accepted + Rejected)) AS 'Waiting',
	(SELECT COUNT(*) FROM Manta.Transactions as [tran] JOIN Manta.Messages as [msg] ON [tran].MessageId = [msg].MessageId WHERE [msg].MtaSendId = [send].MtaSendId AND [tran]TransactionStatusId = 5) AS 'Throttled',
	(SELECT COUNT(*) FROM Manta.Transactions as [tran] JOIN Manta.Messages as [msg] ON [tran].MessageId = [msg].MessageId WHERE [msg].MtaSendId = [send].MtaSendId AND [tran]TransactionStatusId = 1) AS 'Deferred',
	(SELECT MAX(CreatedAt) FROM Manta.Transactions as [tran] JOIN  Manta.Messages as [msg] ON [tran].MessageId = [msg].MessageId WHERE [msg].MtaSendId = [send].MtaSendId) AS 'LastTransactionTimestamp'
FROM Manta.MtaSend as [send]
WHERE ([send].Messages - (Accepted + Rejected)) > 0
ORDER BY [send].CreatedAt DESC", CreateAndFillSendInfo, cmd =>
			{
				cmd.CommandTimeout = 90;
			});

			return new SendInfoCollection(results);
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="sendID"></param>
		/// <returns></returns>
		public SendInfo GetSend(string sendID)
		{
			return _mantaDb.GetSingleObjectFromDatabase(@"
SELECT [snd].*,
	Messages,
	Accepted,
	Rejected,
	([snd].Messages - (Accepted + Rejected)) AS 'Waiting',
	(SELECT COUNT(*) FROM Manta.Transactions as [tran] with(nolock) JOIN Manta.Messages as [msg] with(nolock) ON [tran].MessageId = [msg].MessageId WHERE [msg].MtaSendId = [snd].MtaSendId AND [tran].TransactionStatusId = 5) AS 'Throttled',
	(SELECT COUNT(*) FROM Manta.Transactions as [tran] with(nolock) JOIN Manta.Messages as [msg] with(nolock) ON [tran].MessageId = [msg].MessageId WHERE [msg].MtaSendId = [snd].MtaSendId AND [tran].TransactionStatusId = 1) AS 'Deferred',
	(SELECT MAX(CreatedAt) FROM Manta.Transactions as [tran] with(nolock) JOIN  Manta.Messages as [msg] with(nolock) ON [tran].MessageId = [msg].MessageId WHERE [msg].MtaSendId = [snd].MtaSendId) AS 'LastTransactionTimestamp'
FROM Manta.MtaSend as [snd] with(nolock)
WHERE [snd].SendId = @sndID", CreateAndFillSendInfo, cmd => cmd.Parameters.AddWithValue("@sndID", sendID));
		}

		/// <summary>
		/// Gets a Sends Metadata from the database.
		/// </summary>
		/// <param name="sendID"></param>
		/// <returns></returns>
		public SendMetadataCollection GetSendMetaData(int internalSendID)
		{
			var results = _mantaDb.GetCollectionFromDatabase(@"
SELECT *
FROM Manta.SendMetadata
WHERE MtaSendId = @sndID", CreateAndFillSendMetadata, cmd => cmd.Parameters.AddWithValue("@sndID", internalSendID));

			return new SendMetadataCollection(results);
		}

		/// <summary>
		/// Creates a send info object filled with data from the data record.
		/// </summary>
		/// <param name="record">Where to get the data to fill object from.</param>
		/// <returns>A populated SendInfo object.</returns>
		private SendInfo CreateAndFillSendInfo(IDataRecord record)
		{
			SendInfo sInfo = new SendInfo
			{
				ID = record.GetString("SendId"),
				InternalID = record.GetInt32("MtaSendId"),
				SendStatus = (SendStatus)record.GetInt64("SendStatusId"),
				LastAccessedTimestamp = DateTimeOffset.UtcNow,
				CreatedTimestamp = record.GetDateTime("CreatedAt"),
				Accepted = record.GetInt64("Accepted"),
				Deferred = record.GetInt64("Deferred"),
				Rejected = record.GetInt64("Rejected"),
				Throttled = record.GetInt64("Throttled"),
				TotalMessages = record.GetInt64("Messages"),
				Waiting = record.GetInt64("Waiting")
			};

			if (sInfo.Waiting < 0)
				sInfo.Waiting = 0;

			if (!record.IsDBNull("LastTransactionTimestamp"))
				sInfo.LastTransactionTimestamp = record.GetDateTime("LastTransactionTimestamp");

			return sInfo;
		}

		/// <summary>
		/// Creates a send metadata object from the data record.
		/// </summary>
		/// <param name="record">Where to get the data to fill object from.</param>
		/// <returns>A populated SendMetadata object.</returns>
		private SendMetadata CreateAndFillSendMetadata(IDataRecord record)
		{
			return new SendMetadata
			{
				Name = record.GetStringOrEmpty("Name"),
				Value = record.GetStringOrEmpty("Value")
			};
		}

		public bool SaveSendMetadata(int internalSendID, SendMetadata metadata)
		{
			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = @"IF EXISTS(SELECT 1
FROM Manta.SendMetadata
WHERE MtaSendId = @sndID
AND Name = @name)
	BEGIN
		UPDATE Manta.SendMetadata
		SET Value = @value
		WHERE MtaSendId = @sndID
		AND Name = @name
	END
ELSE
	BEGIN
		INSERT INTO Manta.SendMetadata(MtaSendId, Name, Value)
		VALUES(@sndID, @name, @value)
	END";
				cmd.Parameters.AddWithValue("@sndID", internalSendID);
				cmd.Parameters.AddWithValue("@name", metadata.Name);
				cmd.Parameters.AddWithValue("@value", metadata.Value);
				conn.Open();
				cmd.ExecuteNonQuery();
			}

			return true;
		}
	}
}