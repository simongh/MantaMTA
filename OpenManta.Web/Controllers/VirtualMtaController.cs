using System.Collections.Generic;
using System.Web.Mvc;
using OpenManta.Data;
using OpenManta.Core;
using WebInterface.Models;
using OpenManta.WebLib;

namespace WebInterface.Controllers
{
    public class VirtualMtaController : Controller
    {
        //
        // GET: /VirtualMta/
        public ActionResult Index()
        {
			var ips = VirtualMtaDB.GetVirtualMtas();
			var summary = new List<VirtualMTASummary>();
			var ipGroups = VirtualMtaWebManager.GetAllVirtualMtaGroups();
			foreach (var address in ips)
			{
				summary.Add(new VirtualMTASummary 
				{ 
					IpAddress = address, 
						SendTransactionSummaryCollection = OpenManta.WebLib.DAL.VirtualMtaTransactionDB.GetSendSummaryForIpAddress(address.ID)
				});
			}
			return View(new VirtualMtaPageModel { VirtualMTASummaryCollection = summary.ToArray(), IpGroups = ipGroups });
        }

		//
		// GET: /VirtualMta/Edit
		public ActionResult Edit(int id = WebInterfaceParameters.VIRTUALMTA_NEW_ID)
		{
			if (id == WebInterfaceParameters.VIRTUALMTA_NEW_ID)
				return View(new VirtualMTA());
			return View(VirtualMtaDB.GetVirtualMta(id));
		}

		//
		// GET: /VirtualMta/EditGroup
		public ActionResult EditGroup(int id = WebInterfaceParameters.VIRTUALMTAGROUP_NEW_ID)
		{
			VirtualMtaGroup grp = null;
			if (id == WebInterfaceParameters.VIRTUALMTAGROUP_NEW_ID)
				grp = new VirtualMtaGroup();
			else
				grp = VirtualMtaWebManager.GetVirtualMtaGroup(id);

			return View(new VirtualMtaGroupCreateEditModel
			{
				VirtualMtaGroup = grp,
				VirtualMTACollection = VirtualMtaDB.GetVirtualMtas()
			});
		}
    }
}
