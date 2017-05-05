using System.Net.Mail;
using System.Threading.Tasks;
using OpenManta.Core;

namespace OpenManta.Framework.Smtp
{
	internal interface IMantaSmtpClientPoolCollection
	{
		Task<MantaOutboundClientSendResult> SendAsync(MailAddress mailFrom, MailAddress rcptTo, VirtualMtaGroup vMtaGroup, MXRecord[] mxRecord, string msg);
	}
}