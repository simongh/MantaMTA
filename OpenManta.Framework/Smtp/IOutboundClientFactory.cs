using OpenManta.Core;

namespace OpenManta.Framework.Smtp
{
	internal interface IOutboundClientFactory
	{
		IMantaOutboundClientPool GetOutboundClientPool(VirtualMTA vmta, MXRecord mxRecord);

		IMantaOutboundClient GetOutboundClient(VirtualMTA vmta, MXRecord mxRecord);
	}
}