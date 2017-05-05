﻿using System.Linq;
using System.Web.Http;
using OpenManta.Core;
using OpenManta.Data;
using OpenManta.WebLib;
using WebInterface.Models.API.v1;

namespace WebInterface.Controllers.API.v1
{
	/// <summary>
	/// Summary description for OutboundRules API
	/// </summary>
	[RoutePrefix("api/v1/OutboundRules")]
	public class OutboundRulesController : ApiController
	{
		private readonly IOutboundRuleWebManager _manager;

		public OutboundRulesController(IOutboundRuleWebManager manager)
		{
			Guard.NotNull(manager, nameof(manager));

			_manager = manager;
		}

		[HttpPost]
		[Route("Update")]
		public bool Update(UpdateOutboundRuleViewModel viewModel)
		{
			if (viewModel.VirtualMTA == -1)
				viewModel.VirtualMTA = null;

			OutboundMxPattern pattern = null;
			if (viewModel.PatternID == WebInterfaceParameters.OUTBOUND_RULES_NEW_PATTERN_ID)
				pattern = new OutboundMxPattern();
			else
				pattern = OutboundRuleDB.GetOutboundRulePatterns().SingleOrDefault(p => p.ID == viewModel.PatternID);
			if (pattern == null)
				return false;

			pattern.Description = viewModel.Description.Trim();
			pattern.LimitedToOutboundIpAddressID = viewModel.VirtualMTA;
			pattern.Name = viewModel.Name.Trim();
			pattern.Type = viewModel.Type;
			pattern.Value = viewModel.PatternValue;
			pattern.ID = _manager.Save(pattern);

			_manager.Save(new OutboundRule(pattern.ID, OutboundRuleType.MaxConnections, viewModel.MaxConnections.ToString()));
			_manager.Save(new OutboundRule(pattern.ID, OutboundRuleType.MaxMessagesConnection, viewModel.MaxMessagesConn.ToString()));
			_manager.Save(new OutboundRule(pattern.ID, OutboundRuleType.MaxMessagesPerHour, viewModel.MaxMessagesHour.ToString()));

			return true;
		}
	}
}