using System.Net.Mail;
using System.Threading.Tasks;

namespace OpenManta.Framework.Smtp
{
	internal interface IMantaOutboundClientPool
	{
		long LastUsedTimestamp { get; }

		Task<MantaOutboundClientSendResult> SendAsync(MailAddress mailFrom, MailAddress rcptTo, string msg);
	}
}