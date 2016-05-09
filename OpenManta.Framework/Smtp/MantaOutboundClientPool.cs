using OpenManta.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace OpenManta.Framework.Smtp
{
    internal class MantaOutboundClientPool
    {
        private readonly ICollection<MantaOutboundClient> SmtpClients;
        private readonly int? MaxMessagesMinute;
        private readonly int? MaxConnections;
        private IList<long> SentMessagesLog;
        private readonly MXRecord MXRecord;
        private readonly VirtualMTA VirtualMTA;
        private long _LastUsedTimestamp;
        public long LastUsedTimestamp
        {
            get
            {
                return _LastUsedTimestamp;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vmta"></param>
        /// <param name="mxRecord"></param>
        /// <param name="maxConnections"></param>
        public MantaOutboundClientPool(VirtualMTA vmta, MXRecord mxRecord)
        {
            _LastUsedTimestamp = DateTime.UtcNow.Ticks;
            MXRecord = mxRecord;
            VirtualMTA = vmta;

            var maxMessagesHour = OutboundRuleManager.GetMaxMessagesDestinationHour(vmta, mxRecord);
            if (maxMessagesHour > 0)
            {
                MaxMessagesMinute = (int?)Math.Floor(maxMessagesHour / 60d);
                SentMessagesLog = new List<long>();
                Logging.Debug("MantaOutboundClientPool> for: " + vmta.IPAddress + "-" + mxRecord.Host + " MAX MESSAGES MIN: " + MaxMessagesMinute);
            }
            else
            {
                MaxMessagesMinute = null;
                SentMessagesLog = null;
            }

            var maxConnections = OutboundRuleManager.GetMaxConnectionsToDestination(vmta, mxRecord);
            if (maxConnections > 0)
            {
                MaxConnections = maxConnections;
                Logging.Debug("MantaOutboundClientPool> for: " + vmta.IPAddress + "-" + mxRecord.Host + " MAX CONNECTION: " + MaxConnections);
            }
            else
                MaxConnections = null;

            SmtpClients = new List<MantaOutboundClient>();
        }

        private object sentMessagesLogLock = new object();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawMessage"></param>
        /// <returns></returns>
        public async Task<MantaOutboundClientSendResult> SendAsync(MailAddress mailFrom, MailAddress rcptTo, string msg)
        {
            Logging.Debug("MantaOutboundClientPool.SendAsync> From: " + mailFrom + " To: " + rcptTo);
            _LastUsedTimestamp = DateTime.UtcNow.Ticks;
            if (MaxMessagesMinute.HasValue)
            {
                lock (sentMessagesLogLock)
                {
                    var minuteAgo = DateTime.UtcNow.AddMinutes(-1).Ticks;
                    SentMessagesLog = SentMessagesLog.Where(l => l > minuteAgo).ToList();

                    if (SentMessagesLog.Count >= MaxMessagesMinute)
                    {
                        Logging.Debug("MantaOutboundClientPool.SendAsync> MaxMessagesMinute!");
                        return new MantaOutboundClientSendResult(MantaOutboundClientResult.MaxMessages, null, VirtualMTA, MXRecord);
                    }
                }
            }

            var client = GetClient();
            if (client == null)
            {
                Logging.Debug("MantaOutboundClientPool.SendAsync> MaxConnections!");
                return new MantaOutboundClientSendResult(MantaOutboundClientResult.MaxConnections, null, VirtualMTA, MXRecord);
            }

            var result = await client.SendAsync(mailFrom, rcptTo, msg);
            if (MaxMessagesMinute.HasValue && result.MantaOutboundClientResult == MantaOutboundClientResult.Success)
            {
                lock (sentMessagesLogLock)
                {
                    SentMessagesLog.Add(DateTime.UtcNow.Ticks);
                }
            }

            return result;
        }

        private object GetClientLock = new object();
        private MantaOutboundClient GetClient()
        {
            lock (GetClientLock)
            {
                var client = SmtpClients.FirstOrDefault(c => !c.InUse);
                if (client == null)
                {
                    if (MaxConnections.HasValue == false || SmtpClients.Count < MaxConnections)
                    {
                        client = new MantaOutboundClient(VirtualMTA, MXRecord);
                        SmtpClients.Add(client);
                    }
                }

                if (client != null)
                    client.InUse = true;     
                           
                return client;
            }
        }
    }
}
