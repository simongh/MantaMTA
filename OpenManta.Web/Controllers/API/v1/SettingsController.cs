using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using OpenManta.Core;
using OpenManta.Data;
using WebInterface.Models.API.v1;

namespace WebInterface.Controllers.API.v1
{
	/// <summary>
	/// Summary description for Settings API
	/// </summary>
	[RoutePrefix("api/v1/Settings")]
	public class SettingsController : ApiController
	{
		private readonly ICfgLocalDomains _localDomains;
		private readonly ICfgPara _config;
		private readonly ICfgRelayingPermittedIP _configPermittedIP;

		public SettingsController(ICfgLocalDomains localDomains, ICfgPara config, ICfgRelayingPermittedIP configPermittedIP)
		{
			Guard.NotNull(localDomains, nameof(localDomains));
			Guard.NotNull(config, nameof(config));
			Guard.NotNull(configPermittedIP, nameof(configPermittedIP));

			_localDomains = localDomains;
			_config = config;
			_configPermittedIP = configPermittedIP;
		}

		/// <summary>
		/// Saves the settings.
		/// </summary>
		/// <param name="viewModel"></param>
		/// <returns>TRUE if updated or FALSE if update failed.</returns>
		[HttpPost]
		[Route("Update")]
		public bool Update(UpdateSettingsViewModel viewModel)
		{
			if (viewModel.ClientIdleTimeout < 0 ||
				viewModel.ReceiveTimeout < 0 ||
				viewModel.SendTimeout < 0)
				return false;

			List<IPAddress> relayingIps = new List<IPAddress>();
			foreach (string str in viewModel.IpAddressesForRelaying)
			{
				relayingIps.Add(IPAddress.Parse(str));
			}

			_config.ClientIdleTimeout = viewModel.ClientIdleTimeout;
			_config.ReceiveTimeout = viewModel.ReceiveTimeout;
			_config.SendTimeout = viewModel.SendTimeout;
			_config.DefaultVirtualMtaGroupID = viewModel.DefaultVirtualMtaGroupID;
			_config.EventForwardingHttpPostUrl = viewModel.EventUrl;
			_config.DaysToKeepSmtpLogsFor = viewModel.DaysToKeepSmtpLogsFor;
			_config.MaxTimeInQueueMinutes = viewModel.MaxTimeInQueueHours * 60;
			_config.RetryIntervalBaseMinutes = viewModel.RetryIntervalBase;
			_configPermittedIP.SetRelayingPermittedIPAddresses(relayingIps.ToArray());
			_config.ReturnPathDomainId = viewModel.ReturnPathLocalDomainID;

			var domains = _localDomains.GetLocalDomainsArray();
			_localDomains.ClearLocalDomains();
			foreach (string localDomain in viewModel.LocalDomains)
			{
				if (string.IsNullOrWhiteSpace(localDomain))
					continue;
				LocalDomain ld = domains.SingleOrDefault(d => d.Hostname.Equals(localDomain, StringComparison.OrdinalIgnoreCase));
				if (ld == null)
					ld = new LocalDomain { Hostname = localDomain.Trim() };
				_localDomains.Save(ld);
			}

			return true;
		}
	}
}