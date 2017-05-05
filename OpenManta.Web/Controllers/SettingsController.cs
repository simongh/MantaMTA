using System.Web.Mvc;
using OpenManta.Core;
using OpenManta.Data;
using WebInterface.Models;

namespace WebInterface.Controllers
{
	public class SettingsController : Controller
	{
		private readonly ICfgLocalDomains _localDomains;
		private readonly ICfgPara _config;

		public SettingsController(ICfgLocalDomains localDomains, ICfgPara config)
		{
			Guard.NotNull(localDomains, nameof(localDomains));
			Guard.NotNull(config, nameof(config));

			_localDomains = localDomains;
			_config = config;
		}

		//
		// GET: /Settings/
		public ActionResult Index()
		{
			return View(new SettingsModel
			{
				ClientIdleTimeout = _config.ClientIdleTimeout,
				DaysToKeepSmtpLogsFor = _config.DaysToKeepSmtpLogsFor,
				DefaultVirtualMtaGroupID = _config.DefaultVirtualMtaGroupID,
				VirtualMtaGroupCollection = VirtualMtaGroupDB.GetVirtualMtaGroups(),
				EventForwardingUrl = _config.EventForwardingHttpPostUrl,
				LocalDomains = _localDomains.GetLocalDomainsArray(),
				MaxTimeInQueue = _config.MaxTimeInQueueMinutes,
				ReceiveTimeout = _config.ReceiveTimeout,
				RelayingPermittedIPs = CfgRelayingPermittedIP.GetRelayingPermittedIPAddresses(),
				RetryInterval = _config.RetryIntervalBaseMinutes,
				ReturnPathDomain = _config.ReturnPathDomainId.ToString(),
				SendTimeout = _config.SendTimeout
			});
		}
	}
}