using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using OpenManta.Core;

namespace OpenManta.Framework.Smtp
{
	public interface ISmtpStreamHandler
	{
		IPAddress LocalAddress { get; set; }

		/// <summary>
		/// The remote address is the source of the client request.
		/// </summary>
		IPAddress RemoteAddress { get; set; }

		void Open(TcpClient client);

		void Open(Stream stream);

		string ReadAllLines(bool log = true);

		Task<string> ReadAllLinesAsync(bool log = true);

		Task<string> ReadLineAsync(bool log = true);

		void SetSmtpTransportMIME(SmtpTransportMIME mime);

		void WriteLine(string message, bool log = true);

		Task<bool> WriteLineAsync(string message, bool log = true);

		Task<bool> WriteAsync(string message, bool log = true);
	}
}