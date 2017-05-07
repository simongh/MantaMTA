using OpenManta.Core;

namespace OpenManta.Framework
{
	public interface IBounceRulesManager
	{
		BounceRulesCollection BounceRules { get; }

		BouncePair ConvertSmtpCodeToMantaBouncePair(int smtpCode);

		BouncePair ConvertNdrCodeToMantaBouncePair(string ndrCode);
	}
}