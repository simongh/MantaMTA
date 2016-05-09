using OpenManta.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace OpenManta.Framework.Smtp
{
    internal class MantaSmtpClientPoolCollection : IStopRequired
    {
        private static MantaSmtpClientPoolCollection _Instance = new MantaSmtpClientPoolCollection();
        private object AddClientPoolLock = new object();

        /// <summary>
        /// 
        /// </summary>
        private MantaSmtpClientPoolCollection()
        {
            MantaCoreEvents.RegisterStopRequiredInstance(this);

            ClientPools = new Dictionary<string, MantaOutboundClientPool>();
            Task.Factory.StartNew(async () =>
            {
                do
                {
                    await Task.Delay(1000);
                    foreach (var k in ClientPools.Keys)
                    {
                        if (IsStopping)
                            break;

                        if (ClientPools[k].LastUsedTimestamp < DateTime.UtcNow.AddMinutes(-2).Ticks)
                            ClientPools.Remove(k);
                    }
                } while (!IsStopping);
            }, TaskCreationOptions.LongRunning);
        }

        public static MantaSmtpClientPoolCollection Instance { get { return _Instance; } }
        private IDictionary<string, MantaOutboundClientPool> ClientPools { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vMtaGroup"></param>
        /// <param name="mxRecord"></param>
        /// <param name="rawMsg"></param>
        /// <returns></returns>
        public async Task<MantaOutboundClientSendResult> SendAsync(MailAddress mailFrom, MailAddress rcptTo, VirtualMtaGroup vMtaGroup, MXRecord[] mxRecord, string msg)
        {
            Logging.Debug("MantaOutboundClientPoolCollection.SendAsync> From: " + mailFrom + " To:" + rcptTo + " via:" + vMtaGroup.Name);
            var result = null as MantaOutboundClientSendResult;

            foreach (var vmta in vMtaGroup.VirtualMtaCollection)
            {
                result = await SendAsync(mailFrom, rcptTo, vmta, mxRecord.OrderBy(mx => mx.Preference).First(), msg);
                switch (result.MantaOutboundClientResult)
                {
                    case MantaOutboundClientResult.FailedToConnect:
                    case MantaOutboundClientResult.RejectedByRemoteServer:
                    case MantaOutboundClientResult.Success:
                        return result;
                    case MantaOutboundClientResult.ClientAlreadyInUse:
                    case MantaOutboundClientResult.MaxConnections:
                    case MantaOutboundClientResult.MaxMessages:
                    case MantaOutboundClientResult.ServiceNotAvalible:
                        continue;
                }
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vmta"></param>
        /// <param name="mxRecord"></param>
        /// <param name="rawMsg"></param>
        /// <returns></returns>
        private async Task<MantaOutboundClientSendResult> SendAsync(MailAddress mailFrom, MailAddress rcptTo, VirtualMTA vmta, MXRecord mxRecord, string msg)
        {
            var key = GetPoolKey(vmta, mxRecord);
            if (!ClientPools.ContainsKey(key))
                AddClientPool(vmta, mxRecord);

            var pool = ClientPools[key];
            return await pool.SendAsync(mailFrom, rcptTo, msg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vmta"></param>
        /// <param name="mxRecord"></param>
        private void AddClientPool(VirtualMTA vmta, MXRecord mxRecord)
        {
            var key = GetPoolKey(vmta, mxRecord);

            lock (AddClientPoolLock)
            {
                if (!ClientPools.ContainsKey(key))
                    ClientPools[key] = new MantaOutboundClientPool(vmta, mxRecord);
            }
        }

        private string GetPoolKey(VirtualMTA vmta, MXRecord mxRecord)
        {
            return vmta.ID.ToString() + "-" + mxRecord.Host.ToLower();
        }

        public bool IsStopping = false;
        public void Stop()
        {
            IsStopping = true;
        }
    }
}