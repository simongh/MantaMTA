using System;
using System.Threading.Tasks;
using OpenManta.Core;

namespace OpenManta.Data
{
	public static class MtaMessageDBFactory
	{
		public static IMtaMessageDB Instance { get; internal set; }
	}

	internal class MtaMessageDB : IMtaMessageDB
	{
		/// <summary>
		/// Delimiter user for RCPT addresses.
		/// </summary>
		private const string _RcptToDelimiter = ",";

		private readonly IMantaDB _mantaDb;

		public MtaMessageDB(IMantaDB mantaDb)
		{
			Guard.NotNull(mantaDb, nameof(mantaDb));

			_mantaDb = mantaDb;
		}

		public async Task<string> GetMailFrom(Guid messageId)
		{
			using (var conn = _mantaDb.GetSqlConnection())
			{
				var cmd = conn.CreateCommand();
				cmd.CommandText = @"SELECT TOP 1 MailFrom
FROM Manta.Messages
WHERE MessageId = @msgId";
				cmd.Parameters.AddWithValue("@msgId", messageId);
				await conn.OpenAsync().ConfigureAwait(false);
				var result = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
				if (result == null)
					return string.Empty;
				return result.ToString();
			}
		}
	}
}