using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using log4net;
using OpenManta.Core;

namespace OpenManta.Framework.Smtp
{
	internal class MantaOutboundClientPool : IMantaOutboundClientPool
	{
		private readonly ICollection<IMantaOutboundClient> SmtpClients;
		private readonly int? MaxMessagesMinute;
		private readonly int? MaxConnections;
		private IList<long> SentMessagesLog;
		private readonly MXRecord MXRecord;
		private readonly VirtualMTA VirtualMTA;
		private long _LastUsedTimestamp;
		private object GetClientLock = new object();
		private readonly ILog _logging;
		private object sentMessagesLogLock = new object();
		private readonly IOutboundClientFactory _clientFactory;

		public long LastUsedTimestamp
		{
			get
			{
				return _LastUsedTimestamp;
			}
		}

		public MantaOutboundClientPool(IOutboundRuleManager outboundRulesManager, ILog logging, IOutboundClientFactory clientFactory, VirtualMTA vmta, MXRecord mxRecord)
		{
			Guard.NotNull(outboundRulesManager, nameof(outboundRulesManager));
			Guard.NotNull(logging, nameof(logging));
			Guard.NotNull(clientFactory, nameof(clientFactory));
			Guard.NotNull(vmta, nameof(vmta));
			Guard.NotNull(mxRecord, nameof(mxRecord));

			_logging = logging;
			_clientFactory = clientFactory;

			_LastUsedTimestamp = DateTime.UtcNow.Ticks;
			MXRecord = mxRecord;
			VirtualMTA = vmta;

			var maxMessagesHour = outboundRulesManager.GetMaxMessagesDestinationHour(vmta, mxRecord);
			if (maxMessagesHour > 0)
			{
				MaxMessagesMinute = (int?)Math.Floor(maxMessagesHour / 60d);
				SentMessagesLog = new List<long>();
				_logging.Debug("MantaOutboundClientPool> for: " + vmta.IPAddress + "-" + mxRecord.Host + " MAX MESSAGES MIN: " + MaxMessagesMinute);
			}
			else
			{
				MaxMessagesMinute = null;
				SentMessagesLog = null;
			}

			var maxConnections = outboundRulesManager.GetMaxConnectionsToDestination(vmta, mxRecord);
			if (maxConnections > 0)
			{
				MaxConnections = maxConnections;
				_logging.Debug("MantaOutboundClientPool> for: " + vmta.IPAddress + "-" + mxRecord.Host + " MAX CONNECTION: " + MaxConnections);
			}
			else
				MaxConnections = null;

			SmtpClients = new List<IMantaOutboundClient>();
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="rawMessage"></param>
		/// <returns></returns>
		public async Task<MantaOutboundClientSendResult> SendAsync(MailAddress mailFrom, MailAddress rcptTo, string msg)
		{
			_logging.Debug("MantaOutboundClientPool.SendAsync> From: " + mailFrom + " To: " + rcptTo);
			_LastUsedTimestamp = DateTime.UtcNow.Ticks;
			if (MaxMessagesMinute.HasValue)
			{
				lock (sentMessagesLogLock)
				{
					var minuteAgo = DateTime.UtcNow.AddMinutes(-1).Ticks;
					SentMessagesLog = SentMessagesLog.Where(l => l > minuteAgo).ToList();

					if (SentMessagesLog.Count >= MaxMessagesMinute)
					{
						_logging.Debug("MantaOutboundClientPool.SendAsync> MaxMessagesMinute!");
						return new MantaOutboundClientSendResult(MantaOutboundClientResult.MaxMessages, null, VirtualMTA, MXRecord);
					}
				}
			}

			var client = GetClient();
			if (client == null)
			{
				_logging.Debug("MantaOutboundClientPool.SendAsync> MaxConnections!");
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

		private IMantaOutboundClient GetClient()
		{
			lock (GetClientLock)
			{
				var client = SmtpClients.FirstOrDefault(c => !c.InUse);
				if (client == null)
				{
					if (MaxConnections.HasValue == false || SmtpClients.Count < MaxConnections)
					{
						client = _clientFactory.GetOutboundClient(VirtualMTA, MXRecord);
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