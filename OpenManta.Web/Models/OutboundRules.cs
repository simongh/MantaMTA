using System.Collections.Generic;
using OpenManta.Core;

namespace WebInterface.Models
{
	/// <summary>
	/// Holds the Model for the Outbound Rules page.
	/// </summary>
	public class OutboundRuleModel
	{
		/// <summary>
		/// Collection of the Outbound Rules
		/// </summary>
		public IList<OutboundRule> OutboundRules { get; set; }
		
		/// <summary>
		/// The MX pattern that the rules relate to.
		/// </summary>
		public OutboundMxPattern Pattern { get; set; }

		/// <summary>
		/// Holds a list of all the outbound Virtual MTAs.
		/// </summary>
		public IList<VirtualMTA> VirtualMtaCollection { get; set; }

		public OutboundRuleModel( IList<OutboundRule>  outboundRuleCollection, OutboundMxPattern pattern, IList<VirtualMTA> virtualMtaCollection)
		{
			OutboundRules = outboundRuleCollection;
			Pattern = pattern;
			VirtualMtaCollection = virtualMtaCollection;
		}
	}
}