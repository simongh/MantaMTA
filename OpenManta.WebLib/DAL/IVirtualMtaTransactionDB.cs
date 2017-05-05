using OpenManta.WebLib.BO;

namespace OpenManta.WebLib.DAL
{
	public interface IVirtualMtaTransactionDB
	{
		SendTransactionSummaryCollection GetSendSummaryForIpAddress(int ipAddressId);
	}
}