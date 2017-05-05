using System.Threading.Tasks;
using OpenManta.Core;

namespace OpenManta.Framework
{
	public interface IEventsManager
	{
		EmailProcessingDetails ProcessBounceEmail(string message);

		EmailProcessingDetails ProcessFeedbackLoop(string content);

		Task<int> SaveAsync(MantaEvent evt);

		bool ProcessSmtpResponseMessage(string response, string rcptTo, int internalSendID, out EmailProcessingDetails bounceIdentification);
	}
}