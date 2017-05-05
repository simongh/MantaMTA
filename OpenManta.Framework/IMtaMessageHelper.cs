using System.Threading.Tasks;
using OpenManta.Core;

namespace OpenManta.Framework
{
	public interface IMtaMessageHelper
	{
		void HandleDeliveryDeferral(MtaQueuedMessage msg, string defMsg, VirtualMTA ipAddress, MXRecord mxRecord, bool isServiceUnavailable = false);

		Task<bool> HandleDeliveryDeferralAsync(MtaQueuedMessage msg, string defMsg, VirtualMTA ipAddress, MXRecord mxRecord, bool isServiceUnavailable = false, int? overrideTimeminutes = null);

		Task<bool> HandleDeliveryFailAsync(MtaQueuedMessage msg, string failMsg, VirtualMTA ipAddress, MXRecord mxRecord);

		Task<bool> HandleDeliverySuccessAsync(MtaQueuedMessage msg, VirtualMTA ipAddress, MXRecord mxRecord, string response);

		Task<bool> HandleFailedToConnectAsync(MtaQueuedMessage msg, VirtualMTA ipAddress, MXRecord mxRecord);

		Task<bool> HandleMessageDiscardAsync(MtaQueuedMessage msg);

		Task HandleSendPaused(MtaQueuedMessage msg);

		Task<bool> HandleDeliveryThrottleAsync(MtaQueuedMessage msg, VirtualMTA ipAddress, MXRecord mxRecord);

		Task<bool> HandleServiceUnavailableAsync(MtaQueuedMessage msg, VirtualMTA ipAddress);
	}
}