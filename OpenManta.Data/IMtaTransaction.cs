using System;
using System.Threading.Tasks;
using OpenManta.Core;

namespace OpenManta.Data
{
	public interface IMtaTransaction
	{
		Task<bool> HasBeenHandledAsync(Guid messageID);

		void LogTransaction(MtaMessage msg, TransactionStatus status, string svrResponse, VirtualMTA ipAddress, MXRecord mxRecord);

		Task<bool> LogTransactionAsync(MtaMessage msg, TransactionStatus status, string svrResponse, VirtualMTA ipAddress, MXRecord mxRecord);
	}
}