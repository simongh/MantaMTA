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

            CfgPara.SetClientIdleTimeout(viewModel.ClientIdleTimeout);
            CfgPara.SetReceiveTimeout(viewModel.ReceiveTimeout);
            CfgPara.SetSendTimeout(viewModel.SendTimeout);
            CfgPara.SetDefaultVirtualMtaGroupID(viewModel.DefaultVirtualMtaGroupID);
            CfgPara.SetEventForwardingHttpPostUrl(viewModel.EventUrl);
            CfgPara.SetDaysToKeepSmtpLogsFor(viewModel.DaysToKeepSmtpLogsFor);
            CfgPara.SetMaxTimeInQueueMinutes(viewModel.MaxTimeInQueueHours * 60);
            CfgPara.SetRetryIntervalBaseMinutes(viewModel.RetryIntervalBase);
            CfgRelayingPermittedIP.SetRelayingPermittedIPAddresses(relayingIps.ToArray());
            CfgPara.SetReturnPathLocalDomain(viewModel.ReturnPathLocalDomainID);

            var domains = CfgLocalDomains.GetLocalDomainsArray();
            CfgLocalDomains.ClearLocalDomains();
            foreach (string localDomain in viewModel.LocalDomains)
            {
                if (string.IsNullOrWhiteSpace(localDomain))
                    continue;
                LocalDomain ld = domains.SingleOrDefault(d => d.Hostname.Equals(localDomain, StringComparison.OrdinalIgnoreCase));
                if (ld == null)
                    ld = new LocalDomain { Hostname = localDomain.Trim() };
                CfgLocalDomains.Save(ld);
            }

            return true;
        }
    }
}
