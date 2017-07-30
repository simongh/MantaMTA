using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using OpenManta.Core;

namespace OpenManta.Data
{
	//public static class SendDBFactory
	//{
	//	public static ISendDB Instance { get; internal set; }
	//}

	internal class SendDB : ISendDB
	{
		private readonly IMantaDB _mantaDb;

		public SendDB(IMantaDB mantaDb)
		{
			Guard.NotNull(mantaDb, nameof(mantaDb));
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
			return await _mantaDb.GetSingleObjectFromDatabaseAsync<Send>(@"
BEGIN TRANSACTION

MERGE Manta.MtaSend WITH (HOLDLOCK) AS target
USING (SELECT @sndID) AS source(SendId)
ON (target.SendId = source.SendId)
WHEN NOT MATCHED THEN
	INSERT (SendId, SendStatusId, MtaSendId, CreatedAt)
	VALUES (@sndID, @activeStatusID, ISNULL((SELECT MAX(MtaSendId) + 1 FROM Manta.MtaSend), 1), GETUTCDATE());

COMMIT TRANSACTION

SELECT *
FROM Manta.MtaSend WITH(nolock)
WHERE SendId = @sndID", CreateAndFillSendFromRecord, cmd =>
			{
				cmd.Parameters.AddWithValue("@sndID", sendID);
				cmd.Parameters.AddWithValue("@activeStatusID", (int)SendStatus.Active);
			}).ConfigureAwait(false);
		}

		/// <summary>
		/// Creates a SendID object and fills it with values from the datarecord.
		/// </summary>
		/// <param name="record">Record to get data from.</param>
		/// <returns>SendID filled with data.</returns>
		private Send CreateAndFillSendFromRecord(IDataRecord record)
		{
			Send sendID = new Send();
			sendID.ID = record.GetString("SendId");
			sendID.InternalID = record.GetInt32("MtaSendId");
			sendID.SendStatus = (SendStatus)record.GetInt32("SendStatusId");
			sendID.LastAccessedTimestamp = DateTime.UtcNow;
			sendID.CreatedTimestamp = record.GetDateTime("CreatedAt");

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
			return await _mantaDb.GetSingleObjectFromDatabaseAsync(@"
SELECT *
FROM Manta.MtaSend WITH(NOLOCK)
WHERE MtaSendId = @internalSndID", CreateAndFillSendFromRecord, cmd => cmd.Parameters.AddWithValue("@internalSndID", internalSendID)).ConfigureAwait(false);
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
UPDATE Manta.MtaSend
SET SendStatusId = @sendStatus
WHERE SendId = @sendID";
				cmd.Parameters.AddWithValue("@sendID", sendID);
				cmd.Parameters.AddWithValue("@sendStatus", (int)status);
				conn.Open();
				cmd.ExecuteNonQuery();
			}
		}
	}
}