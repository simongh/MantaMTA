using System;
using System.Net.Mail;
using System.Threading.Tasks;
using OpenManta.Core;

namespace OpenManta.Framework.Smtp
{
	internal interface IMantaOutboundClient : IDisposable
	{
		bool InUse { get; set; }
		MXRecord MXRecord { get; }

		Task<MantaOutboundClientSendResult> SendAsync(MailAddress mailFrom, MailAddress rcptTo, string msg, bool isRetry = false);
	}
}