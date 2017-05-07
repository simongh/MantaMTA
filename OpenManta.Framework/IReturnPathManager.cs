using System;
using System.Threading.Tasks;

namespace OpenManta.Framework
{
	public interface IReturnPathManager
	{
		string GenerateReturnPath(string rcptTo, int internalSendID);

		string GenerateReturnPath(string rcptTo, int internalSendID, string returnDomain);

		bool TryDecode(string returnPath, out string rcptTo, out int internalSendID);

		Task<string> GetReturnPathFromMessageIDAsync(Guid messageID);

		string GetReturnPathFromMessageID(Guid messageID);
	}
}