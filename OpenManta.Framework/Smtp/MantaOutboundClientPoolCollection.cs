using OpenManta.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using log4net;

namespace OpenManta.Framework.Smtp
{
	internal class MantaSmtpClientPoolCollection : IMantaSmtpClientPoolCollection, IStopRequired
	{
		private object AddClientPoolLock = new object();
		public bool IsStopping = false;
		private readonly ILog _logging;
		private IDictionary<string, IMantaOutboundClientPool> _ClientPools;
		private readonly IOutboundClientFactory _clientFactory;

		public MantaSmtpClientPoolCollection(IMantaCoreEvents coreEvents, ILog logging, IOutboundClientFactory clientFactory)
		{
			Guard.NotNull(coreEvents, nameof(coreEvents));
			Guard.NotNull(logging, nameof(logging));
			Guard.NotNull(clientFactory, nameof(clientFactory));

			_logging = logging;
			_clientFactory = clientFactory;

			coreEvents.RegisterStopRequiredInstance(this);

			_ClientPools = new Dictionary<string, IMantaOutboundClientPool>();
			Task.Factory.StartNew(async () =>
			{
				do
				{
					await Task.Delay(1000);
					foreach (var k in _ClientPools.Keys)
					{
						if (IsStopping)
							break;

						if (_ClientPools[k].LastUsedTimestamp < DateTime.UtcNow.AddMinutes(-2).Ticks)
							_ClientPools.Remove(k);
					}
				} while (!IsStopping);
			}, TaskCreationOptions.LongRunning);
		}

		public async Task<MantaOutboundClientSendResult> SendAsync(MailAddress mailFrom, MailAddress rcptTo, VirtualMtaGroup vMtaGroup, MXRecord[] mxRecord, string msg)
		{
			_logging.Debug("MantaOutboundClientPoolCollection.SendAsync> From: " + mailFrom + " To:" + rcptTo + " via:" + vMtaGroup.Name);
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
			if (!_ClientPools.ContainsKey(key))
				AddClientPool(vmta, mxRecord);

			var pool = _ClientPools[key];
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
				if (!_ClientPools.ContainsKey(key))
					_ClientPools[key] = _clientFactory.GetOutboundClientPool(vmta, mxRecord);
			}
		}

		private string GetPoolKey(VirtualMTA vmta, MXRecord mxRecord)
		{
			return vmta.ID.ToString() + "-" + mxRecord.Host.ToLower();
		}

		public void Stop()
		{
			IsStopping = true;
		}
	}
}