using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenManta.Core;

namespace OpenManta.Framework
{
	public interface IMessageManager
	{
		string AddHeader(string message, MessageHeader header);

		string RemoveHeader(string message, string headerName);

		MessageHeaderCollection GetMessageHeaders(string messageData);

		string UnfoldHeaders(string headerSection);
	}
}