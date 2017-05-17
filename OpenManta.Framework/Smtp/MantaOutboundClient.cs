using OpenManta.Core;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Threading.Tasks;
using log4net;

namespace OpenManta.Framework.Smtp
{
	/// <summary>
	/// Handle connection with an SMTP Server.
	/// </summary>
	internal class MantaOutboundClient : IMantaOutboundClient
	{
		private int _MessagesAccepted = 0;
		private int? _MaxMessagesConnection = null;

		private TcpClient TcpClient = null;
		public object _inUseLock = new object();
		private readonly ILog _logging;
		private readonly IMtaParameters _config;

		/// <summary>
		/// Holds the Transport type that should be used for the DATA lines.
		/// </summary>
		private SmtpTransportMIME _DataTransportMime = SmtpTransportMIME._7BitASCII;

		/// <summary>
		/// Will be false until Disposed is called
		/// </summary>
		private bool IsDisposed = false;

		private TcpClient CreateTcpClient()
		{
			var tcp = new TcpClient(new IPEndPoint(_VirtualMta.IPAddress, 0));
			tcp.ReceiveTimeout = _config.Client.ConnectionReceiveTimeoutInterval * 1000;
			tcp.SendTimeout = _config.Client.ConnectionSendTimeoutInterval * 1000;
			tcp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
			return tcp;
		}

		private bool _CanPipeline;

		private MXRecord _MXRecord;
		private VirtualMTA _VirtualMta;

		public bool InUse { get; set; } = false;

		/// <summary>
		/// SMTP stream handler, used to write/read the underlying stream.
		/// </summary>
		private ISmtpStreamHandler SmtpStream;

		/// <summary>
		/// Holds the MX Record that this client is connected to.
		/// </summary>
		public MXRecord MXRecord { get { return _MXRecord; } }

		/// <summary>
		/// Creates a SmtpOutboundClient bound to the specified endpoint.
		/// </summary>
		/// <param name="ipAddress">The local IP address to bind to.</param>
		public MantaOutboundClient(IOutboundRuleManager outboundRules, ILog logging, ISmtpStreamHandler streamHandler, IMtaParameters config, VirtualMTA vmta, MXRecord mx)
		{
			Guard.NotNull(outboundRules, nameof(outboundRules));
			Guard.NotNull(logging, nameof(logging));

			_logging = logging;
			SmtpStream = streamHandler;
			_config = config;

			_VirtualMta = vmta;
			_MXRecord = mx;
			TcpClient = CreateTcpClient();

			_CanPipeline = false;

			_MaxMessagesConnection = outboundRules.GetMaxMessagesPerConnection(mx, vmta);
			if (_MaxMessagesConnection < 1)
				_MaxMessagesConnection = null;
		}

		/// <summary>
		/// Finaliser, ensure dispose is always called.
		/// </summary>
		~MantaOutboundClient()
		{
			if (TcpClient.Connected)
				ExecQuitAsync().GetAwaiter().GetResult();

			if (!IsDisposed)
				this.Dispose(true);
		}

		/// <summary>
		/// Dispose method.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Dispose method.
		/// </summary>
		/// <param name="disposing"></param>
		public void Dispose(bool disposing)
		{
			if (IsDisposed)
				return;

			if (disposing)
				TcpClient.Close();

			IsDisposed = true;
		}

		public async Task<MantaOutboundClientSendResult> SendAsync(MailAddress mailFrom, MailAddress rcptTo, string msg, bool isRetry = false)
		{
			try
			{
				MantaOutboundClientSendResult result = null;
				if (!TcpClient.Connected)
				{
					if (ServiceNotAvailableManager.IsServiceUnavailable(_VirtualMta.IPAddress.ToString(), MXRecord.Host.ToLower()))
						return new MantaOutboundClientSendResult(MantaOutboundClientResult.ServiceNotAvalible, null, _VirtualMta, _MXRecord);

					result = await ConnectAsync();
					if (result.MantaOutboundClientResult != MantaOutboundClientResult.Success)
					{
						if (result.MantaOutboundClientResult == MantaOutboundClientResult.ServiceNotAvalible ||
							result.MantaOutboundClientResult == MantaOutboundClientResult.FailedToConnect)
							ServiceNotAvailableManager.Add(_VirtualMta.IPAddress.ToString(), MXRecord.Host.ToLower(), DateTimeOffset.UtcNow);

						return result;
					}
				}
				else
				{
					result = await ExecRsetAsync();
					if (result.MantaOutboundClientResult != MantaOutboundClientResult.Success)
						return result;
				}

				// MAIL FROM
				result = await ExecMailFromAsync(mailFrom);
				if (result.MantaOutboundClientResult != MantaOutboundClientResult.Success)
					return result;

				// RCPT TO
				result = await ExecRcptToAsync(rcptTo);
				if (result.MantaOutboundClientResult != MantaOutboundClientResult.Success)
					return result;

				// DATA
				result = await ExecDataAsync(msg);

				if (result.MantaOutboundClientResult == MantaOutboundClientResult.Success
				   && _MaxMessagesConnection.HasValue
				   && _MessagesAccepted >= _MaxMessagesConnection)
				{
					// MAX MESSAGES: QUIT
					await ExecQuitAsync();
					_MessagesAccepted = 0;
				}

				return result;
			}
			catch (Exception ex)
			{
				if (ex is ObjectDisposedException)
				{
					if (isRetry)
						return new MantaOutboundClientSendResult(MantaOutboundClientResult.FailedToConnect, "500 Failed to connect", _VirtualMta, _MXRecord);
					else
						return await SendAsync(mailFrom, rcptTo, msg, true);
				}

				return new MantaOutboundClientSendResult(MantaOutboundClientResult.RejectedByRemoteServer, "400 Try again later", _VirtualMta, _MXRecord);
			}
			finally
			{
				lock (_inUseLock)
				{
					InUse = false;
				}
			}
		}

		/// <summary>
		/// Attempt to connect to the specified MX server.
		/// </summary>
		/// <param name="mx">MX Record of the server to connect to.</param>
		private async Task<MantaOutboundClientSendResult> ConnectAsync()
		{
			TcpClient = CreateTcpClient();
			try
			{
				await TcpClient.ConnectAsync(_MXRecord.Host, Client.SMTP_PORT);
			}
			catch (Exception)
			{
				return new MantaOutboundClientSendResult(MantaOutboundClientResult.FailedToConnect, "421 Failed to connect", _VirtualMta, _MXRecord);
			}

			SmtpStream.Open(TcpClient);

			// Read the Server greeting.
			string response = SmtpStream.ReadAllLines();

			// Check we get a valid banner.
			if (!response.StartsWith("2"))
			{
				TcpClient.Close();

				// If the MX is actively denying use service access, SMTP code 421 then we should inform
				// the ServiceNotAvailableManager manager so it limits our attepts to this MX to 1/minute.
				if (response.StartsWith("421"))
					return new MantaOutboundClientSendResult(MantaOutboundClientResult.ServiceNotAvalible, response, _VirtualMta, _MXRecord);

				return new MantaOutboundClientSendResult(MantaOutboundClientResult.FailedToConnect, response, _VirtualMta, _MXRecord);
			}

			// We have connected, so say helli
			return await ExecHeloAsync();
		}

		/// <summary>
		/// Send the data to the server
		/// </summary>
		/// <param name="data">Data to send to the server</param>
		/// <param name="failedCallback">Action to call if fails to send.</param>
		private async Task<MantaOutboundClientSendResult> ExecDataAsync(string data)
		{
			await SmtpStream.WriteLineAsync("DATA");
			string response = await SmtpStream.ReadAllLinesAsync(); // Data response or Mail From if pipelining

			// If the remote MX supports pipelining then we need to check the MAIL FROM and RCPT to responses.
			if (_CanPipeline)
			{
				// Check MAIL FROM OK.
				if (!response.StartsWith("250"))
				{
					await SmtpStream.ReadAllLinesAsync(); // RCPT TO
					await SmtpStream.ReadAllLinesAsync(); // DATA
					return new MantaOutboundClientSendResult(MantaOutboundClientResult.RejectedByRemoteServer, response, _VirtualMta, _MXRecord);
				}

				// Check RCPT TO OK.
				response = await SmtpStream.ReadAllLinesAsync();
				if (!response.StartsWith("250"))
				{
					await SmtpStream.ReadAllLinesAsync(); // DATA
					return new MantaOutboundClientSendResult(MantaOutboundClientResult.RejectedByRemoteServer, response, _VirtualMta, _MXRecord);
				}

				// Get the Data Command response.
				response = await SmtpStream.ReadAllLinesAsync();
			}

			if (!response.StartsWith("354"))
				return new MantaOutboundClientSendResult(MantaOutboundClientResult.RejectedByRemoteServer, response, _VirtualMta, _MXRecord);

			// Send the message data using the correct transport MIME
			SmtpStream.SetSmtpTransportMIME(_DataTransportMime);
			await SmtpStream.WriteAsync(data, false);
			await SmtpStream.WriteAsync(MtaParameters.NewLine + "." + MtaParameters.NewLine, false);

			// Data done so return to 7-Bit mode.
			SmtpStream.SetSmtpTransportMIME(SmtpTransportMIME._7BitASCII);

			response = await SmtpStream.ReadAllLinesAsync();
			if (!response.StartsWith("250"))
				return new MantaOutboundClientSendResult(MantaOutboundClientResult.RejectedByRemoteServer, response, _VirtualMta, _MXRecord);

			_MessagesAccepted++;
			return new MantaOutboundClientSendResult(MantaOutboundClientResult.Success, response, _VirtualMta, _MXRecord);
		}

		/// <summary>
		/// Say EHLO/HELO to the server.
		/// Will also check to see if 8BITMIME is supported.
		/// </summary>
		/// <param name="failedCallback">Action to call if hello fail.</param>
		private async Task<MantaOutboundClientSendResult> ExecHeloAsync()
		{
			// We have connected to the MX, Say EHLO.
			await SmtpStream.WriteLineAsync("EHLO " + _VirtualMta.Hostname);
			string response = await SmtpStream.ReadAllLinesAsync();
			if (response.StartsWith("421"))
				return new MantaOutboundClientSendResult(MantaOutboundClientResult.ServiceNotAvalible, response, _VirtualMta, _MXRecord);

			try
			{
				if (!response.StartsWith("2"))
				{
					// If server didn't respond with a success code on hello then we should retry with HELO
					await SmtpStream.WriteLineAsync("HELO " + _VirtualMta.Hostname);
					response = await SmtpStream.ReadAllLinesAsync();
					if (!response.StartsWith("250"))
					{
						TcpClient.Close();
						return new MantaOutboundClientSendResult(MantaOutboundClientResult.ServiceNotAvalible, response, _VirtualMta, _MXRecord);
					}
				}
				else
				{
					// Server responded to EHLO
					// Check to see if it supports 8BITMIME
					if (response.IndexOf("8BITMIME", StringComparison.OrdinalIgnoreCase) > -1)
						_DataTransportMime = SmtpTransportMIME._8BitUTF;
					else
						_DataTransportMime = SmtpTransportMIME._7BitASCII;

					// Check to see if the server supports pipelining
					_CanPipeline = response.IndexOf("PIPELINING", StringComparison.OrdinalIgnoreCase) > -1;
				}
			}
			catch (IOException)
			{
				// Remote Endpoint Disconnected Mid HELO.
				return new MantaOutboundClientSendResult(MantaOutboundClientResult.ServiceNotAvalible, response, _VirtualMta, _MXRecord);
			}

			return new MantaOutboundClientSendResult(MantaOutboundClientResult.Success, null, _VirtualMta, _MXRecord);
		}

		/// <summary>
		/// Send the MAIL FROM command to the server using <paramref name="mailFrom"/> as parameter.
		/// </summary>
		/// <param name="mailFrom">Email address to use as parameter.</param>
		/// <param name="failedCallback">Action to call if command fails.</param>
		private async Task<MantaOutboundClientSendResult> ExecMailFromAsync(MailAddress mailFrom)
		{
			await SmtpStream.WriteLineAsync("MAIL FROM: <" +
										(mailFrom == null ? string.Empty : mailFrom.Address) + ">" +
										(_DataTransportMime == SmtpTransportMIME._8BitUTF ? " BODY=8BITMIME" : string.Empty));

			// If the remote MX doesn't support pipelining then wait and check the response.
			if (!_CanPipeline)
			{
				string response = await SmtpStream.ReadAllLinesAsync();

				if (!response.StartsWith("250"))
				{
					if (response.StartsWith("421"))
						return new MantaOutboundClientSendResult(MantaOutboundClientResult.ServiceNotAvalible, response, _VirtualMta, _MXRecord);

					return new MantaOutboundClientSendResult(MantaOutboundClientResult.RejectedByRemoteServer, response, _VirtualMta, _MXRecord);
				}
			}

			return new MantaOutboundClientSendResult(MantaOutboundClientResult.Success, null, _VirtualMta, _MXRecord);
		}

		/// <summary>
		/// Send the SMTP Quit command to the Server.
		/// </summary>
		private async Task ExecQuitAsync()
		{
			if (TcpClient.Connected)
			{
				try
				{
					await SmtpStream.WriteLineAsync("QUIT");
					// Don't read response as don't care.
					// Close the TCP connection.
					TcpClient.GetStream().Close();
					TcpClient.Close();
				}
				catch (ObjectDisposedException)
				{
					_logging.Debug("SmtpOutboundClient: Tried to quit an already disposed client.");
				}
			}
		}

		/// <summary>
		/// Send the RCPT TO command to the server using <paramref name="rcptTo"/> as parameter.
		/// </summary>
		/// <param name="rcptTo">Email address to use as parameter.</param>
		/// <param name="failedCallback">Action to call if command fails.</param>
		private async Task<MantaOutboundClientSendResult> ExecRcptToAsync(MailAddress rcptTo)
		{
			await SmtpStream.WriteLineAsync("RCPT TO: <" + rcptTo.Address + ">");

			// If the remote MX doesn't support pipelining then wait and check the response.
			if (!_CanPipeline)
			{
				string response = await SmtpStream.ReadAllLinesAsync();

				if (!response.StartsWith("250"))
					return new MantaOutboundClientSendResult(MantaOutboundClientResult.RejectedByRemoteServer, response, _VirtualMta, _MXRecord);
			}

			return new MantaOutboundClientSendResult(MantaOutboundClientResult.Success, null, _VirtualMta, _MXRecord);
		}

		/// <summary>
		/// Send the RSET command to the server.
		/// </summary>
		private async Task<MantaOutboundClientSendResult> ExecRsetAsync()
		{
			if (!await SmtpStream.WriteLineAsync("RSET"))
				throw new ObjectDisposedException("Connection");

			var response = await SmtpStream.ReadAllLinesAsync();
			switch (response[0])
			{
				case '2':
					return new MantaOutboundClientSendResult(MantaOutboundClientResult.Success, response, _VirtualMta, _MXRecord);

				case '4':
				case '5':
				default:
					return new MantaOutboundClientSendResult(MantaOutboundClientResult.RejectedByRemoteServer, response, _VirtualMta, _MXRecord);
			}
		}
	}
}