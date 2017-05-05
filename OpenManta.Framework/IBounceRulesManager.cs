using OpenManta.Core;

namespace OpenManta.Framework
{
	public interface IBounceRulesManager
	{
		BounceRulesCollection BounceRules { get; }
	}
}