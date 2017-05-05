using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using OpenManta.Core;
using OpenManta.Data;
using OpenManta.Framework;
using OpenManta.WebLib;
using WebInterface.Models;

namespace WebInterface.Controllers
{
	public class OutboundRulesController : Controller
	{
		private readonly IOutboundRuleWebManager _manager;
		private readonly IVirtualMtaDB _virtuaMtalDb;
		private readonly IOutboundRuleDB _ruleDb;

		public OutboundRulesController(IOutboundRuleWebManager manager, IVirtualMtaDB virtualMtaDb, IOutboundRuleDB ruleDb)
		{
			Guard.NotNull(manager, nameof(manager));
			Guard.NotNull(virtualMtaDb, nameof(virtualMtaDb));
			Guard.NotNull(ruleDb, nameof(ruleDb));

			_manager = manager;
			_virtuaMtalDb = virtualMtaDb;
			_ruleDb = ruleDb;
		}

		//
		// GET: /OutboundRules/
		public ActionResult Index()
		{
			return View(_ruleDb.GetOutboundRulePatterns());
		}

		//
		// GET: /OutboundRules/Edit?id=
		public ActionResult Edit(int id = WebInterfaceParameters.OUTBOUND_RULES_NEW_PATTERN_ID)
		{
			OutboundMxPattern pattern = null;
			IList<OutboundRule> rules = null;

			if (id != WebInterfaceParameters.OUTBOUND_RULES_NEW_PATTERN_ID)
			{
				pattern = _ruleDb.GetOutboundRulePatterns().Single(p => p.ID == id);
				rules = _ruleDb.GetOutboundRules().Where(r => r.OutboundMxPatternID == id).ToList();
			}
			else
			{
				pattern = new OutboundMxPattern();
				rules = _ruleDb.GetOutboundRules().Where(r => r.OutboundMxPatternID == MtaParameters.OUTBOUND_RULES_DEFAULT_PATTERN_ID).ToList();
			}

			IList<VirtualMTA> vMtas = _virtuaMtalDb.GetVirtualMtas();
			return View(new OutboundRuleModel(rules, pattern, vMtas));
		}

		//
		// GET: /OutboundRules/Delete?patternID=
		public ActionResult Delete(int patternID)
		{
			_manager.Delete(patternID);
			return View();
		}
	}
}