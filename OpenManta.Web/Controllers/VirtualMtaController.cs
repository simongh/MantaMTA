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
		private readonly IVirtualMtaWebManager _manager;
		private readonly OpenManta.WebLib.DAL.IVirtualMtaTransactionDB _virtualtransactionsDb;

		public VirtualMtaController(IVirtualMtaWebManager manager, OpenManta.WebLib.DAL.IVirtualMtaTransactionDB virtualtransactionsDb)
		{
			Guard.NotNull(manager, nameof(manager));
			Guard.NotNull(virtualtransactionsDb, nameof(virtualtransactionsDb));

			_manager = manager;
			_virtualtransactionsDb = virtualtransactionsDb;
		}

		//
		// GET: /VirtualMta/
		public ActionResult Index()
		{
			var ips = VirtualMtaDB.GetVirtualMtas();
			var summary = new List<VirtualMTASummary>();
			var ipGroups = _manager.GetAllVirtualMtaGroups();
			foreach (var address in ips)
			{
				summary.Add(new VirtualMTASummary
				{
					IpAddress = address,
					SendTransactionSummaryCollection = _virtualtransactionsDb.GetSendSummaryForIpAddress(address.ID)
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
				grp = _manager.GetVirtualMtaGroup(id);

			return View(new VirtualMtaGroupCreateEditModel
			{
				VirtualMtaGroup = grp,
				VirtualMTACollection = VirtualMtaDB.GetVirtualMtas()
			});
		}
	}
}