using OpenManta.Core;

namespace OpenManta.Framework
{
	internal interface IThrottleManager
	{
		bool TryGetSendAuth(VirtualMTA ipAddress, MXRecord mxRecord);
	}
}