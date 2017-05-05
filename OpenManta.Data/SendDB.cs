using System;
using System.Data;
using System.Data.SqlClient;
using OpenManta.Core;
using System.Threading.Tasks;

namespace OpenManta.Data
{
	public static class SendDBFactory
	{
		public static ISendDB Instance { get; internal set; }
	}

	internal class SendDB : ISendDB
	{
		private readonly IDataRetrieval _dataRetrieval;
		private readonly IMantaDB _mantaDb;

		public SendDB(IDataRetrieval dataRetrieval, IMantaDB mantaDb)
		{
			Guard.NotNull(dataRetrieval, nameof(dataRetrieval));
			Guard.NotNull(mantaDb, nameof(mantaDb));

			_dataRetrieval = dataRetrieval;
			_mantaDb = mantaDb;
		}

		/// <summary>
		/// Gets the sendID's internal ID from the database. If the record doesn't exist
		/// then it will be created.
		/// </summary>
		/// <param name="sendID">The SendID to get the internal ID for.</param>
		/// <returns></returns>
		public async Task<Send> CreateAndGetInternalSendIDAsync(string sendID)
		{
			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = @"
BEGIN TRANSACTION

MERGE man_mta_send WITH (HOLDLOCK) AS target
USING (SELECT @sndID) AS source(mta_send_id)
ON (target.mta_send_id = source.mta_send_id)
WHEN NOT MATCHED THEN
	INSERT (mta_send_id, mta_sendStatus_id, mta_send_internalId, mta_send_createdTimestamp)
	VALUES (@sndID, @activeStatusID, ISNULL((SELECT MAX(mta_send_internalID) + 1 FROM man_mta_send), 1), GETUTCDATE());

COMMIT TRANSACTION

SELECT *
FROM man_mta_send WITH(nolock)
WHERE mta_send_id = @sndID";
				cmd.Parameters.AddWithValue("@sndID", sendID);
				cmd.Parameters.AddWithValue("@activeStatusID", (int)SendStatus.Active);
				return await _dataRetrieval.GetSingleObjectFromDatabaseAsync(cmd, CreateAndFillSendFromRecord).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Creates a SendID object and fills it with values from the datarecord.
		/// </summary>
		/// <param name="record">Record to get data from.</param>
		/// <returns>SendID filled with data.</returns>
		private Send CreateAndFillSendFromRecord(IDataRecord record)
		{
			Send sendID = new Send();
			sendID.ID = record.GetString("mta_send_id");
			sendID.InternalID = record.GetInt32("mta_send_internalId");
			sendID.SendStatus = (SendStatus)record.GetInt32("mta_sendStatus_id");
			sendID.LastAccessedTimestamp = DateTime.UtcNow;
			sendID.CreatedTimestamp = record.GetDateTime("mta_send_createdTimestamp");

			return sendID;
		}

		/// <summary>
		/// Gets the specified send.
		/// </summary>
		/// <param name="internalSendID">Internal ID of the Send to get.</param>
		/// <returns>The specified Send or NULL if none with the ID exist.</returns>
		public Send GetSend(int internalSendID)
		{
			return GetSendAsync(internalSendID).Result;
		}

		/// <summary>
		/// Gets the specified send.
		/// </summary>
		/// <param name="internalSendID">Internal ID of the Send to get.</param>
		/// <returns>The specified Send or NULL if none with the ID exist.</returns>
		public async Task<Send> GetSendAsync(int internalSendID)
		{
			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = @"
SELECT *
FROM man_mta_send WITH(NOLOCK)
WHERE mta_send_internalId = @internalSndID";
				cmd.Parameters.AddWithValue("@internalSndID", internalSendID);
				return await _dataRetrieval.GetSingleObjectFromDatabaseAsync<Send>(cmd, CreateAndFillSendFromRecord);
			}
		}

		/// <summary>
		/// Sets the status of the specified send to the specified status.
		/// </summary>
		/// <param name="sendID">ID of the send to set the staus of.</param>
		/// <param name="status">The status to set the send to.</param>
		public void SetSendStatus(string sendID, SendStatus status)
		{
			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = @"
UPDATE man_mta_send
SET mta_sendStatus_id = @sendStatus
WHERE mta_send_id = @sendID";
				cmd.Parameters.AddWithValue("@sendID", sendID);
				cmd.Parameters.AddWithValue("@sendStatus", (int)status);
				conn.Open();
				cmd.ExecuteNonQuery();
			}
		}
	}
}