using OpenManta.Core;

namespace OpenManta.Framework.Smtp
{
    public enum MantaOutboundClientResult : int
    {
        MaxConnections = -3,
        MaxMessages = -2,
        ClientAlreadyInUse = -1,
        Success = 0,
        FailedToConnect = 1,
        ServiceNotAvalible = 2,
        RejectedByRemoteServer = 3
    }

    public class MantaOutboundClientSendResult
    {
        public MantaOutboundClientResult MantaOutboundClientResult { get; set; }
        public string Message { get; set; }
        public VirtualMTA VirtualMTA { get; set; }
        public MXRecord MXRecord { get; set; }

        public MantaOutboundClientSendResult(MantaOutboundClientResult result, string message, VirtualMTA vmta = null, MXRecord mx = null)
        {
            MantaOutboundClientResult = result;
            Message = message;
            VirtualMTA = vmta;
            MXRecord = mx;
        }
    }
}
