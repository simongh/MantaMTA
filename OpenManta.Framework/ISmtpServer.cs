using System;
using System.Net;

namespace OpenManta.Framework
{
	public interface ISmtpServer : IDisposable
	{
		void Open(IPAddress ipAddress, int port);
	}
}