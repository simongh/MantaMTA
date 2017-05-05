using System.Linq;
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
		private readonly ICfgRelayingPermittedIP _configPermittedIP;
		private readonly IVirtualMtaGroupDB _virtualGroupDb;

		public SettingsController(ICfgLocalDomains localDomains, ICfgPara config, ICfgRelayingPermittedIP configPermittedIP, IVirtualMtaGroupDB virtualGroupDb)
		{
			Guard.NotNull(localDomains, nameof(localDomains));
			Guard.NotNull(config, nameof(config));
			Guard.NotNull(configPermittedIP, nameof(configPermittedIP));
			Guard.NotNull(virtualGroupDb, nameof(virtualGroupDb));

			_localDomains = localDomains;
			_config = config;
			_configPermittedIP = configPermittedIP;
			_virtualGroupDb = virtualGroupDb;
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
				VirtualMtaGroupCollection = _virtualGroupDb.GetVirtualMtaGroups(),
				EventForwardingUrl = _config.EventForwardingHttpPostUrl,
				LocalDomains = _localDomains.GetLocalDomainsArray(),
				MaxTimeInQueue = _config.MaxTimeInQueueMinutes,
				ReceiveTimeout = _config.ReceiveTimeout,
				RelayingPermittedIPs = _configPermittedIP.GetRelayingPermittedIPAddresses().ToArray(),
				RetryInterval = _config.RetryIntervalBaseMinutes,
				ReturnPathDomain = _config.ReturnPathDomainId.ToString(),
				SendTimeout = _config.SendTimeout
			});
		}
	}
}