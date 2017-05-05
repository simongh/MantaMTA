using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenManta.Core;

namespace OpenManta.Framework
{
	public interface ISmtpServerTransaction
	{
		string Data { get; set; }
		bool HasMailFrom { get; }
		string MailFrom { get; set; }
		MessageDestination MessageDestination { get; set; }
		IList<string> RcptTo { get; set; }
		SmtpTransportMIME TransportMIME { get; set; }

		void AddHeader(string name, string value);

		Task<SmtpServerTransactionAsyncResult> SaveAsync();
	}
}