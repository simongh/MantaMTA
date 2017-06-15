using System.Data.SqlClient;
using OpenManta.Core;
using OpenManta.WebLib.BO;
using OpenManta.Data;

namespace OpenManta.WebLib.DAL
{
	internal class VirtualMtaTransactionDB : IVirtualMtaTransactionDB
	{
		private readonly IMantaDB _mantaDb;

		public VirtualMtaTransactionDB(IMantaDB mantaDb)
		{
			Guard.NotNull(mantaDb, nameof(mantaDb));

			_mantaDb = mantaDb;
		}

		/// <summary>
		/// Gets a summary of a virtual MTAs transaction history.
		/// </summary>
		/// <param name="ipAddressId"></param>
		/// <returns></returns>
		public SendTransactionSummaryCollection GetSendSummaryForIpAddress(int ipAddressId)
		{
			var results = _mantaDb.GetCollectionFromDatabase(@"
SELECT TransactionStatusId, COUNT(*) AS 'Count'
FROM Manta.Transactions
WHERE IpAddressId = @ipAddressId
GROUP BY TransactionStatusId", CreateAndFillSendTransactionSummaryFromRecord, cmd => cmd.Parameters.AddWithValue("@ipAddressId", ipAddressId));

			return new SendTransactionSummaryCollection(results);
		}

		/// <summary>
		/// Creates a SendTransactionSummary from the DataRecord.
		/// </summary>
		/// <param name="record">The record of the data.</param>
		/// <returns>A filled SendTransactionSummary object.</returns>
		private SendTransactionSummary CreateAndFillSendTransactionSummaryFromRecord(System.Data.IDataRecord record)
		{
			return new SendTransactionSummary((TransactionStatus)record.GetInt64("TransactionStatusId"), record.GetInt64("count"));
		}
	}
}