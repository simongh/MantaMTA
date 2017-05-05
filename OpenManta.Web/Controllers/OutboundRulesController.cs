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

		public OutboundRulesController(IOutboundRuleWebManager manager)
		{
			Guard.NotNull(manager, nameof(manager));

			_manager = manager;
		}

		//
		// GET: /OutboundRules/
		public ActionResult Index()
		{
			return View(OutboundRuleDB.GetOutboundRulePatterns());
		}

		//
		// GET: /OutboundRules/Edit?id=
		public ActionResult Edit(int id = WebInterfaceParameters.OUTBOUND_RULES_NEW_PATTERN_ID)
		{
			OutboundMxPattern pattern = null;
			IList<OutboundRule> rules = null;

			if (id != WebInterfaceParameters.OUTBOUND_RULES_NEW_PATTERN_ID)
			{
				pattern = OutboundRuleDB.GetOutboundRulePatterns().Single(p => p.ID == id);
				rules = OutboundRuleDB.GetOutboundRules().Where(r => r.OutboundMxPatternID == id).ToList();
			}
			else
			{
				pattern = new OutboundMxPattern();
				rules = OutboundRuleDB.GetOutboundRules().Where(r => r.OutboundMxPatternID == MtaParameters.OUTBOUND_RULES_DEFAULT_PATTERN_ID).ToList();
			}

			IList<VirtualMTA> vMtas = VirtualMtaDB.GetVirtualMtas();
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