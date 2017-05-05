using System.Net.Sockets;

namespace OpenManta.Framework.Smtp
{
	public interface ISmtpServerFactory
	{
		ISmtpStreamHandler GetHandler(TcpClient client);

		ISmtpServerTransaction GetTransaction();
	}
}