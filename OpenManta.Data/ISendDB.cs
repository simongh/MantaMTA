using System.Threading.Tasks;
using OpenManta.Core;

namespace OpenManta.Data
{
	public interface ISendDB
	{
		Task<Send> CreateAndGetInternalSendIDAsync(string sendID);

		Send GetSend(int internalSendID);

		Task<Send> GetSendAsync(int internalSendID);

		void SetSendStatus(string sendID, SendStatus status);
	}
}