using OpenManta.Core;
using OpenManta.Data;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace OpenManta.Framework
{
	public class SendManager
	{
		private readonly ISendDB _sendDb;

		/// <summary>
		/// Collection of cached Sends, key is SendID.
		/// Objects should be in this and _SendsInternalID with alternate key.
		/// </summary>
		private ConcurrentDictionary<string, Send> _Sends = new ConcurrentDictionary<string, Send>();

		/// <summary>
		/// Collection of cached Sends, key is internal ID.
		/// Objects should be in this and _Sends with alternate key.
		/// </summary>
		private ConcurrentDictionary<int, Send> _SendsInternalID = new ConcurrentDictionary<int, Send>();

		/// <summary>
		/// Timestamp of when cached sends were last cleared.
		/// </summary>
		private DateTimeOffset _SendsLastCleared = DateTimeOffset.UtcNow;

		/// <summary>
		/// Sends cache lock, used when clearing the cached items.
		/// </summary>
		private object _SendsLock = new object();

		static SendManager()
		{
			Instance = new SendManager(SendDBFactory.Instance);
		}

		private SendManager(ISendDB sendDb)
		{
			Guard.NotNull(sendDb, nameof(sendDb));

			_sendDb = sendDb;
		}

		public static SendManager Instance { get; private set; }

		/// <summary>
		/// Clear the Sends from memory.
		/// </summary>
		public void ClearSendsCache()
		{
			lock (_SendsLock)
			{
				this._Sends.Clear();
				this._SendsInternalID.Clear();
				this._SendsLastCleared = DateTimeOffset.UtcNow;
			}
		}

		/// <summary>
		/// Gets Send with the specified sendID.
		/// If it doesn't exist it will be created in the database.
		/// </summary>
		/// <param name="sendId">ID of the Send.</param>
		/// <returns>The Send.</returns>
		public Send GetSend(string sendId)
		{
			return GetSendAsync(sendId).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Gets Send with the specified sendID.
		/// If it doesn't exist it will be created in the database.
		/// </summary>
		/// <param name="sendId">ID of the Send.</param>
		/// <returns>The Send.</returns>
		public async Task<Send> GetSendAsync(string sendId)
		{
			// Don't want send IDs sitting in memory for to long so clear every so often.
			if (this._SendsLastCleared.AddSeconds(60) < DateTimeOffset.UtcNow)
				this.ClearSendsCache();

			Send snd;

			// Try to get the send id from the cached collection.
			if (!this._Sends.TryGetValue(sendId, out snd))
			{
				// Doesn't exist so need to create or load from datbase.
				snd = await _sendDb.CreateAndGetInternalSendIDAsync(sendId).ConfigureAwait(false);

				// Add are new item to the cache.
				this._Sends.TryAdd(sendId, snd);
				this._SendsInternalID.TryAdd(snd.InternalID, snd);
			}

			// return the value.
			return snd;
		}

		/// <summary>
		/// Gets the specified Send.
		/// </summary>
		/// <param name="internalSendID">Internal ID of the Send.</param>
		/// <returns>The Send.</returns>
		public async Task<Send> GetSendAsync(int internalSendID)
		{
			// Don't want send IDs sitting in memory for to long so clear every so often.
			if (this._SendsLastCleared.AddSeconds(10) < DateTimeOffset.UtcNow)
				this.ClearSendsCache();

			Send snd;

			// Try to get the send id from the cached collection.
			if (!this._SendsInternalID.TryGetValue(internalSendID, out snd))
			{
				// Doesn't exist so need to create or load from datbase.
				snd = await _sendDb.GetSendAsync(internalSendID);

				// Add are new item to the cache.
				this._SendsInternalID.TryAdd(internalSendID, snd);
				this._Sends.TryAdd(snd.ID, snd);
			}

			return snd;
		}

		/// <summary>
		/// Gets the default send ID, based of the current time.
		/// </summary>
		/// <returns></returns>
		internal async Task<Send> GetDefaultInternalSendIdAsync()
		{
			string sendID = DateTimeOffset.UtcNow.ToString("yyyyMMdd");
			return await this.GetSendAsync(sendID);
		}

		/// <summary>
		/// Sets the status of the specified send to the specified status.
		/// </summary>
		/// <param name="sendID">ID of the send to set the staus of.</param>
		/// <param name="status">The status to set the send to.</param>
		public void SetSendStatus(string sendID, SendStatus status)
		{
			_sendDb.SetSendStatus(sendID, status);
		}
	}
}