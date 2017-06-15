using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using OpenManta.Core;
using OpenManta.Data;
using OpenManta.WebLib.BO;

namespace OpenManta.WebLib.DAL
{
	internal class TransactionDB : ITransactionDB
	{
		private readonly IMantaDB _mantaDb;

		public TransactionDB(IMantaDB mantaDb)
		{
			Guard.NotNull(mantaDb, nameof(mantaDb));

			_mantaDb = mantaDb;
		}

		/// <summary>
		/// Gets information about the speed of a send.
		/// </summary>
		/// <param name="sendID">ID of the send to get speed information about.</param>
		/// <returns>SendSpeedInfo</returns>
		public SendSpeedInfo GetSendSpeedInfo(string sendID)
		{
			var results = _mantaDb.GetCollectionFromDatabase(@"
DECLARE @internalSendID int
SELECT @internalSendID = MtaSendId
FROM Manta.MtaSend
WHERE SendId = @sndID

SELECT COUNT(*) AS 'Count', [tran].TransactionStatusId, CONVERT(smalldatetime, [tran].CreatedAt) as 'CreatedAt'
FROM Manta.Transactions as [tran] with(nolock)
JOIN Manta.Messages AS [msg] with(nolock) ON [tran].MessageId = [msg].MessageId
WHERE [msg].MtaSendId = @internalSendID
GROUP BY [tran].TransactionStatusId, CONVERT(smalldatetime, [tran].CreatedAt)
ORDER BY CONVERT(smalldatetime, [tran].CreatedAt)", CreateAndFillSendSpeedInfoItemFromRecord, cmd => cmd.Parameters.AddWithValue("@sndID", sendID));

			return new SendSpeedInfo(results);
		}

		/// <summary>
		/// Gets information about the speed of sending over the last one hour.
		/// </summary>
		/// <returns>SendSpeedInfo</returns>
		public SendSpeedInfo GetLastHourSendSpeedInfo()
		{
			var results = _mantaDb.GetCollectionFromDatabase(@"
SELECT COUNT(*) AS 'Count', [tran].TransactionStatusId, CONVERT(smalldatetime, [tran].CreatedAt) as 'CreatedAt'
FROM Manta.Transactions as [tran] WITH (nolock)
WHERE [tran].CreatedAt >= DATEADD(HOUR, -1, GETUTCDATE())
GROUP BY [tran].TransactionStatusId, CONVERT(smalldatetime, [tran].CreatedAt)
ORDER BY CONVERT(smalldatetime, [tran].CreatedAt)", CreateAndFillSendSpeedInfoItemFromRecord);

			return new SendSpeedInfo(results);
		}

		/// <summary>
		/// Creates a SendSpeedInfoItem object and fills it with data from the data record.
		/// </summary>
		/// <param name="record">Contains the data to use for filling.</param>
		/// <returns>A SendSpeedInfoItem object filled with data from the record.</returns>
		private SendSpeedInfoItem CreateAndFillSendSpeedInfoItemFromRecord(IDataRecord record)
		{
			SendSpeedInfoItem item = new SendSpeedInfoItem
			{
				Count = record.GetInt64("Count"),
				Status = (TransactionStatus)record.GetInt64("TransactionStatusId"),
				Timestamp = record.GetDateTime("CreatedAt")
			};
			return item;
		}

		private string GetSkip(string fieldName, int pageNumber, int pageSize)
		{
			return $"{fieldName} >= {((pageNumber * pageSize) - pageSize) + 1} AND {fieldName} <= {pageNumber * pageSize}";
		}

		/// <summary>
		/// Gets a data page about bounces from the transactions table for a send.
		/// </summary>
		/// <param name="sendID">Send to get data for.</param>
		/// <param name="pageNum">The page to get.</param>
		/// <param name="pageSize">The size of the data pages.</param>
		/// <returns>An array of BounceInfo from the data page.</returns>
		public IEnumerable<BounceInfo> GetBounceInfo(string sendID, int pageNum, int pageSize)
		{
			bool hasSendID = !string.IsNullOrWhiteSpace(sendID);

			return _mantaDb.GetCollectionFromDatabase((hasSendID ? @"
declare @internalSendID int
SELECT @internalSendID = MtaSendId
FROM Manta.MtaSend WITH(nolock)
WHERE MtaSendId = @sndID
" : string.Empty) + @"
SELECT [sorted].*
FROM (
		SELECT ROW_NUMBER() OVER (ORDER BY count(*) DESC, ServerHostname) as 'Row',
			   TransactionStatusId,
			   ServerResponse,
			   ServerHostname as 'ServerHostname',
			   [ip].Hostname,
			   [ip].IpAddress, COUNT(*) as 'Count',
			   MAX(CreatedAt) as 'LastOccurred'
		FROM Manta.Transactions as [tran] with(nolock)
		JOIN Manta.Messages as [msg] with(nolock) ON [tran].MessageId = [msg].MessageId
		JOIN Manta.IpAddresses as [ip] ON [tran].IpAddressId = [ip].IpAddressId
		WHERE TransactionStatusId IN (1, 2, 3, 6) --// Todo: Make this enum!
		" + (hasSendID ? "AND [msg].MtaSendId = @internalSendID " : string.Empty) + @"
		GROUP BY TransactionStatusId, ServerResponse, ServerHostname,[ip].Hostname, [ip].IpAddress
	 ) as [sorted]
WHERE " + GetSkip("[Row]", pageNum, pageSize), CreateAndFillBounceInfo, cmd =>
			 {
				 if (hasSendID)
					 cmd.Parameters.AddWithValue("@sndID", sendID);
			 });
		}

		/// <summary>
		/// Gets a data page about bounces from the transactions table for a send.
		/// </summary>
		/// <param name="sendID">Send to get data for.</param>
		/// <param name="pageNum">The page to get.</param>
		/// <param name="pageSize">The size of the data pages.</param>
		/// <returns>An array of BounceInfo from the data page.</returns>
		public IEnumerable<BounceInfo> GetFailedInfo(string sendID, int pageNum, int pageSize)
		{
			bool hasSendID = !string.IsNullOrWhiteSpace(sendID);

			return _mantaDb.GetCollectionFromDatabase((hasSendID ? @"
declare @internalSendID int
SELECT @internalSendID = MtaSendId
FROM Manta.MtaSend WITH(nolock)
WHERE SendId = @sndID
" : string.Empty) + @"
SELECT [sorted].*
FROM (
		SELECT ROW_NUMBER() OVER (ORDER BY COUNT(*) DESC, ServerHostname) as 'Row',
			   TransactionStatusId,
			   ServerResponse,
			   ServerHostname',
			   [ip].Hostname,
			   [ip].IpAddress, COUNT(*) as 'Count',
			   MAX(CreatedAt) as 'LastOccurred'
		FROM Manta.Transactions as [tran] WITH(nolock)
		JOIN Manta.Messages as [msg] WITH(nolock) ON [tran].MessageId = [msg].MessageId
		JOIN Manta.IpAddresses as [ip] ON [tran].IpAddressId = [ip].IpAddressId
		WHERE TransactionStatusId IN (2, 3, 6) --// Todo: Make this enum!
		" + (hasSendID ? "AND [msg].MtaSendId = @internalSendID " : string.Empty) + @"
		GROUP BY TransactionStatusId, ServerResponse,ServerHostname,[ip].Hostname, [ip].IpAddress
	 ) as [sorted]
WHERE " + GetSkip("[Row]", pageNum, pageSize), CreateAndFillBounceInfo, cmd =>
			  {
				  if (hasSendID)
					  cmd.Parameters.AddWithValue("@sndID", sendID);
			  });
		}

		/// <summary>
		/// Gets a data page about bounces from the transactions table for a send.
		/// </summary>
		/// <param name="sendID">Send to get data for.</param>
		/// <param name="pageNum">The page to get.</param>
		/// <param name="pageSize">The size of the data pages.</param>
		/// <returns>An array of BounceInfo from the data page.</returns>
		public IEnumerable<BounceInfo> GetDeferralInfo(string sendID, int pageNum, int pageSize)
		{
			bool hasSendID = !string.IsNullOrWhiteSpace(sendID);

			return _mantaDb.GetCollectionFromDatabase((hasSendID ? @"
declare @internalSendID int
SELECT @internalSendID = MtaSendId
FROM Manta.MtaSend
WHERE SendId = @sndID
" : string.Empty) + @"
SELECT [sorted].*
FROM (
		SELECT ROW_NUMBER() OVER (ORDER BY COUNT(*) DESC, ServerHostname) as 'Row',
			   TransactionStatusId,
			   ServerResponse,
			   ServerHostname,
			   [ip].Hostname,
			   [ip].IpAddress, COUNT(*) as 'Count',
			   MAX(CreatedAt) as 'LastOccurred'
		FROM Manta.Transactions as [tran] WITH(nolock)
		JOIN Manta.Messages as [msg] WITH(nolock) ON [tran].MessageId = [msg].MessageId
		JOIN Manta.IpAddresses as [ip] ON [tran].IpAddressId = [ip].IpAddressId
		WHERE TransactionStatusId IN (1) --// Todo: Make this enum!
		" + (hasSendID ? "AND [msg].MtaSendId = @internalSendID " : string.Empty) + @"
		GROUP BY TransactionStatusId, ServerResponse, ServerHostname,[ip].Hostname, [ip].IpAddress
	 ) as [sorted]
WHERE " + GetSkip("[Row]", pageNum, pageSize), CreateAndFillBounceInfo, cmd =>
			   {
				   if (hasSendID)
					   cmd.Parameters.AddWithValue("@sndID", sendID);
			   });
		}

		/// <summary>
		/// Gets the most common bounces from the last hour.
		/// </summary>
		/// <param name="count">Amount of bounces to get.</param>
		/// <returns>Information about the bounces</returns>
		public IEnumerable<BounceInfo> GetLastHourBounceInfo(int count)
		{
			return _mantaDb.GetCollectionFromDatabase($@"
SELECT TOP {count} ROW_NUMBER() OVER (ORDER BY COUNT(*) DESC, ServerHostname) as 'Row',
			   TransactionStatusId,
			   ServerResponse,
			   ServerHostname,
			   [ip].Hostname,
			   [ip].IpAddress, COUNT(*) as 'Count',
			   MAX(CreatedAt) as 'LastOccurred'
FROM Manta.Transactions as [tran] WITH (nolock)
JOIN Manta.Messages as [msg] WITH(nolock) ON [tran].MessageId = [msg].MessageId
JOIN Manta.IpAddresses as [ip] ON [tran].IpAddressId = [ip].IpAddressId
WHERE [tran].CreatedAt >= DATEADD(HOUR, -1, GETUTCDATE())
AND TransactionStatusId IN (1, 2, 3, 6)
AND ServerHostname NOT LIKE ''
GROUP BY TransactionStatusId, ServerResponse, ServerHostname,[ip].Hostname, [ip].IpAddress
ORDER BY COUNT(*) DESC", CreateAndFillBounceInfo);
		}

		/// <summary>
		/// Counts the total amount of bounces for a send.
		/// </summary>
		/// <param name="sendID">ID of the send to count bounces for.</param>
		/// <returns>The amount of bounces for the send.</returns>
		public int GetBounceCount(string sendID)
		{
			bool hasSendID = !string.IsNullOrWhiteSpace(sendID);
			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = (hasSendID ? @"
declare @internalSendID int
SELECT @internalSendID = MtaSendId
FROM Manta.MtaSend
WHERE SendId = @sndID
" : string.Empty) + @"
SELECT COUNT(*)
FROM(
SELECT 1 as 'Col'
		FROM Manta.Transactions as [tran]
		JOIN Manta.Messages as [msg] ON [tran].MessageId = [msg].MessageId
		JOIN Manta.IpAddresses as [ip] ON [tran].IpAddressId = [ip].IpAddressId
		WHERE TransactionStatusId IN (1, 2, 3, 6) --// Todo: Make this enum!
		" + (hasSendID ? "AND [msg].MtaSendId = @internalSendID" : string.Empty) + @"
	GROUP BY TransactionStatusId, ServerResponse, ServerHostname,[ip].Hostname, [ip].IpAddress
	) as [sorted]";
				if (hasSendID)
					cmd.Parameters.AddWithValue("@sndID", sendID);
				conn.Open();
				return Convert.ToInt32(cmd.ExecuteScalar());
			}
		}

		/// <summary>
		/// Counts the total amount of deferrals for a send.
		/// </summary>
		/// <param name="sendID">ID of the send to count bounces for.</param>
		/// <returns>The amount of deferrals for the send.</returns>
		public int GetDeferredCount(string sendID)
		{
			bool hasSendID = !string.IsNullOrWhiteSpace(sendID);
			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = (hasSendID ? @"
declare @internalSendID int
SELECT @internalSendID = MtaSendId
FROM Manta.MtaSend WITH(nolock)
WHERE SendId = @sndID
" : string.Empty) + @"
SELECT COUNT(*)
FROM(
SELECT 1 as 'Col'
		FROM Manta.Transactions as [tran] WITH(nolock)
		JOIN Manta.Messages as [msg] WITH(nolock) ON [tran].MessageId = [msg].MessageId
		JOIN Manta.IpAddresses as [ip] ON [tran].IpAddressId = [ip].IpAddressId
		WHERE TransactionStatusId IN (1) --// Todo: Make this enum!
		" + (hasSendID ? "AND [msg].MtaSendId = @internalSendID" : string.Empty) + @"
	GROUP BY TransactionStatusId, ServerResponse, ServerHostname,[ip].Hostname, [ip].IpAddress
	) as [sorted]";
				if (hasSendID)
					cmd.Parameters.AddWithValue("@sndID", sendID);
				conn.Open();
				return Convert.ToInt32(cmd.ExecuteScalar());
			}
		}

		/// <summary>
		/// Counts the total amount of deferrals for a send.
		/// </summary>
		/// <param name="sendID">ID of the send to count bounces for.</param>
		/// <returns>The amount of deferrals for the send.</returns>
		public int GetFailedCount(string sendID)
		{
			bool hasSendID = !string.IsNullOrWhiteSpace(sendID);
			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = (hasSendID ? @"
declare @internalSendID int
SELECT @internalSendID = MtaSendId
FROM Manta.MtaSend WITH(nolock)
WHERE SendId = @sndID
" : string.Empty) + @"
SELECT COUNT(*)
FROM(
SELECT 1 as 'Col'
		FROM Manta.Transactions as [tran] WITH(nolock)
		JOIN Manta.Messages as [msg] WITH(nolock) ON [tran].MessageId = [msg].MessageId
		JOIN Manta.IpAddresses as [ip] ON [tran].IpAddressId = [ip].IpAddressId
		WHERE TransactionStatusId IN (2, 3, 6) --// Todo: Make this enum!
		" + (hasSendID ? "AND [msg].MtaSendId = @internalSendID" : string.Empty) + @"
	GROUP BY TransactionStatusId, ServerResponse, ServerHostname,[ip].Hostname, [ip].IpAddress
	) as [sorted]";
				if (hasSendID)
					cmd.Parameters.AddWithValue("@sndID", sendID);
				conn.Open();
				return Convert.ToInt32(cmd.ExecuteScalar());
			}
		}

		/// <summary>
		/// Gets the total deferred and rejected counts.
		/// </summary>
		/// <param name="deferred">Returns the deferred count.</param>
		/// <param name="rejected">Returns the rejected count.</param>
		public void GetBounceDeferredAndRejected(out long deferred, out long rejected)
		{
			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = @"
declare @deferred bigint
declare @rejected bigint

SELECT @deferred = COUNT(*)
FROM Manta.Transactions WITH(nolock)
WHERE TransactionStatusId = 1

SELECT @rejected = COUNT(*)
FROM Manta.Transactions WITH(nolock)
WHERE TransactionStatusId IN (2, 3, 6)

SELECT @deferred as 'Deferred', @rejected as 'Rejected'";
				conn.Open();
				SqlDataReader reader = cmd.ExecuteReader();
				reader.Read();
				deferred = Convert.ToInt64(reader["Deferred"]);
				rejected = Convert.ToInt64(reader["Rejected"]);
				reader.Close();
			}
		}

		/// <summary>
		/// Creates a BounceInfo object and fills it with data from the data record.
		/// </summary>
		/// <param name="record">Where to get the data from.</param>
		/// <returns>BounceInfo filled with data from the data record.</returns>
		private BounceInfo CreateAndFillBounceInfo(IDataRecord record)
		{
			BounceInfo bounceInfo = new BounceInfo();
			bounceInfo.Count = record.GetInt64("Count");
			bounceInfo.LocalHostname = record.GetString("Hostname");
			bounceInfo.LocalIpAddress = record.GetString("IpAddress");
			bounceInfo.Message = record.GetString("ServerResponse");
			bounceInfo.RemoteHostname = record.GetStringOrEmpty("ServerHostname");
			bounceInfo.TransactionStatus = (TransactionStatus)record.GetInt64("TransactionStatusId");
			bounceInfo.LastOccurred = record.GetDateTime("LastOccurred");
			return bounceInfo;
		}

		/// <summary>
		/// Gets a summary of the transactions made in the last one hour.
		/// </summary>
		/// <returns>Transaction Summary</returns>
		public SendTransactionSummaryCollection GetLastHourTransactionSummary()
		{
			var results = _mantaDb.GetCollectionFromDatabase(@"
SELECT [tran].TransactionStatusId, COUNT(*) AS 'Count'
FROM Manta.Transactions as [tran] WITH(nolock)
WHERE [tran].CreatedAt >= DATEADD(HOUR, -1, GETUTCDATE())
GROUP BY [tran].TransactionStatusId", CreateAndFillTransactionSummary);

			return new SendTransactionSummaryCollection(results);
		}

		/// <summary>
		/// Creates a SendTransactionSummary page and fills it with data from the record.
		/// </summary>
		/// <param name="record">Record containing the data to fill with.</param>
		/// <returns>The filled SendTransactionSummary object.</returns>
		private SendTransactionSummary CreateAndFillTransactionSummary(IDataRecord record)
		{
			return new SendTransactionSummary
			{
				Count = record.GetInt64("Count"),
				Status = (TransactionStatus)record.GetInt64("TransactionStatusId")
			};
		}
	}
}