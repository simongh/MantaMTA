using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using log4net;
using OpenManta.Core;
using OpenManta.Data;
using OpenManta.Framework.Smtp;

namespace OpenManta.Framework
{
	/// <summary>
	/// Provides a server for receiving SMTP commands/messages.
	/// </summary>
	internal class SmtpServer : Disposable, ISmtpServer
	{
		/// <summary>
		/// Listens to TCP socket.
		/// </summary>
		private TcpListener _TcpListener = null;

		private readonly ILog _logging;
		private readonly ISmtpServerFactory _handlerFactory;
		private readonly IMtaParameters _config;
		private readonly IFeedbackLoopEmailAddressDB _feedbackDb;

		private bool _hasHelo;
		private bool _hasFrom;
		private bool _quitting;
		private ISmtpStreamHandler _smtpStream;
		private ISmtpServerTransaction _mailTransaction;
		private string _host;
		private string _serverHostname;

		public SmtpServer(ILog logging, Smtp.ISmtpServerFactory handlerFactory, IMtaParameters config, IFeedbackLoopEmailAddressDB feedbackDb)
		{
			Guard.NotNull(logging, nameof(logging));
			Guard.NotNull(handlerFactory, nameof(handlerFactory));
			Guard.NotNull(config, nameof(config));
			Guard.NotNull(feedbackDb, nameof(feedbackDb));

			_logging = logging;
			_handlerFactory = handlerFactory;
			_config = config;
			_feedbackDb = feedbackDb;
		}

		/// <summary>
		/// Creates an instance of the Colony101 SMTP Server.
		/// </summary>
		/// <param name="port">Port number that server bind to.</param>
		public void Open(int port)
		{
			Open(IPAddress.Any, port);
		}

		/// <summary>
		/// Creates an instance of the Colony101 SMTP Server.
		/// </summary>
		/// <param name="iPAddress">IP Address to use for binding.</param>
		/// <param name="port">Port number that server bind to.</param>
		public void Open(IPAddress ipAddress, int port)
		{
			// Create the TCP Listener using specified port on all IPs
			_TcpListener = new TcpListener(ipAddress, port);

			try
			{
				_TcpListener.Start();
				_TcpListener.BeginAcceptTcpClient(AsyncConnectionHandler, _TcpListener);
			}
			catch (SocketException ex)
			{
				_logging.Error("Failed to create server on " + ipAddress.ToString() + ":" + port, ex);
				return;
			}

			_logging.Info("Server started on " + ipAddress.ToString() + ":" + port);
		}

		protected override void Dispose(bool disposing)
		{
			if (IsDisposed)
				return;

			_TcpListener.Stop();
			_TcpListener = null;

			IsDisposed = true;
		}

		/// <summary>
		/// Event fired when a new Connection to the SMTP Server is made.
		/// </summary>
		/// <param name="ir">The AsyncResult from the TcpListener.</param>
		private void AsyncConnectionHandler(IAsyncResult ir)
		{
			// If the TCP Listener has been set to null, then we cannot handle any connections.
			if (_TcpListener == null)
				return;

			try
			{
				TcpClient client = _TcpListener.EndAcceptTcpClient(ir);
				_TcpListener.BeginAcceptTcpClient(AsyncConnectionHandler, _TcpListener);
				Task.Factory.StartNew(HandleSmtpConnection, client, TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness);
			}
			catch (ObjectDisposedException)
			{
				// SMTP Server stop was done mid connection handshake, just ignore it.
			}
		}

		/// <summary>
		/// Gets the hostname for the server that is being connected to by client.
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		private async Task<string> GetServerHostnameAsync(TcpClient client)
		{
			string serverIPAddress = (client.Client.LocalEndPoint as IPEndPoint).Address.ToString();
			string serverHost = string.Empty;
			try
			{
				IPHostEntry hostEntry = await Dns.GetHostEntryAsync(serverIPAddress);
				serverHost = hostEntry.HostName;
			}
			catch (Exception)
			{
				// Host doesn't have reverse DNS. Use IP Address.
				serverHost = serverIPAddress;
			}

			return serverHost;
		}

		/// <summary>
		/// Method handles a single connection from a client.
		/// </summary>
		/// <param name="obj">Connection with the client.</param>
		private async Task<bool> HandleSmtpConnection(object obj)
		{
			TcpClient client = (TcpClient)obj;
			client.ReceiveTimeout = _config.Client.ConnectionReceiveTimeoutInterval * 1000;
			client.SendTimeout = _config.Client.ConnectionSendTimeoutInterval * 1000;

			try
			{
				_smtpStream = _handlerFactory.GetHandler(client);
				_serverHostname = await GetServerHostnameAsync(client);

				// Identify our MTA
				await _smtpStream.WriteLineAsync("220 " + _serverHostname + " ESMTP " + MtaParameters.MTA_NAME + " Ready");

				// Set to true when the client has sent quit command.
				_quitting = false;

				// Set to true when the client has said hello.
				_hasHelo = false;

				// Hostname of the client as it self identified in the HELO.
				_host = string.Empty;

				_mailTransaction = null;

				// As long as the client is connected and hasn't sent the quit command then keep accepting commands.
				while (client.Connected && !_quitting)
				{
					// Read the next command. If no line then this will wait for one.
					string cmd = await _smtpStream.ReadLineAsync();

					// Client Disconnected.
					if (cmd == null)
						break;

					#region SMTP Commands that can be run before HELO is issued by client.

					// Handle the QUIT command. Should return 221 and close connection.
					if (await QuitCommandAsync(cmd))
						continue;

					// Reset the mail transaction state. Forget any mail from rcpt to data.
					if (await ResetCommandAsync(cmd))
						continue;

					// Do nothing except return 250. Do nothing just return success (250).
					if (await NoopCommandAsync(cmd))
						continue;

					#endregion SMTP Commands that can be run before HELO is issued by client.

					// EHLO should 500 Bad Command as we don't support enhanced services, clients should then HELO.
					// We need to get the hostname provided by the client as it will be used in the recivied header.
					if (await HelloCommandAsync(cmd))
						continue;

					#region Commands that must be after a HELO

					// Client MUST helo before being allowed to do any of these commands.
					if (!_hasHelo)
					{
						await _smtpStream.WriteLineAsync("503 HELO first");
						continue;
					}

					// Mail From should have a valid email address parameter, if it doesn't system error.
					// Mail From should also begin new transaction and forget any previous mail from, rcpt to or data commands.
					// Do this by creating new instance of SmtpTransaction class.
					if (await MailFromCommandAsync(cmd))
						continue;

					// RCPT TO should have an email address parameter. It can only be set if MAIL FROM has already been set,
					// multiple RCPT TO addresses can be added.
					if (await RcptCommandAsync(cmd))
						continue;

					// Handle the data command, all commands from the client until single line with only '.' should be treated as
					// a single blob of data.
					if (await DataCommandAsync(cmd))
						continue;

					#endregion Commands that must be after a HELO

					// If got this far then we don't known the command.
					await _smtpStream.WriteLineAsync("500 Unknown command");
				}
			}
			catch (System.IO.IOException) { /* Connection timeout */ }
			finally
			{
				// Client has issued QUIT command or connecion lost.
				if (client.GetStream() != null)
					client.GetStream().Close();
				client.Close();
			}

			return true;
		}

		private async Task<bool> QuitCommandAsync(string command)
		{
			if (!command.Equals("QUIT", StringComparison.OrdinalIgnoreCase))
				return false;

			_quitting = true;
			await _smtpStream.WriteLineAsync("221 Goodbye");

			return true;
		}

		private async Task<bool> ResetCommandAsync(string command)
		{
			if (!command.Equals("RSET", StringComparison.OrdinalIgnoreCase))
				return false;

			_mailTransaction = null;
			await _smtpStream.WriteLineAsync("250 Ok");

			return true;
		}

		private async Task<bool> NoopCommandAsync(string command)
		{
			if (!command.Equals("NOOP", StringComparison.OrdinalIgnoreCase))
				return false;

			await _smtpStream.WriteLineAsync("250 Ok");

			return true;
		}

		private async Task<bool> HelloCommandAsync(string command)
		{
			if (!command.StartsWith("HELO", StringComparison.OrdinalIgnoreCase) && !command.StartsWith("EHLO", StringComparison.OrdinalIgnoreCase))
				return false;

			// Helo should be followed by a hostname, if not syntax error.
			if (command.IndexOf(" ") < 0)
			{
				await _smtpStream.WriteLineAsync("501 Syntax error");
				return true;
			}

			// Grab the hostname.
			_host = command.Substring(command.IndexOf(" ")).Trim();

			// There should not be any spaces in the hostname if it is sytax error.
			if (_host.IndexOf(" ") >= 0)
			{
				await _smtpStream.WriteLineAsync("501 Syntax error");
				_host = string.Empty;
				return true;
			}

			// Client has now said hello so set connection variable to true and 250 back to the client.
			_hasHelo = true;
			if (command.StartsWith("HELO", StringComparison.OrdinalIgnoreCase))
			{
				await _smtpStream.WriteLineAsync("250 Hello " + _host + "[" + _smtpStream.RemoteAddress.ToString() + "]");
			}
			else
			{
				// EHLO was sent, let the client know what extensions we support.
				await _smtpStream.WriteLineAsync("250-Hello " + _host + "[" + _smtpStream.RemoteAddress.ToString() + "]");
				await _smtpStream.WriteLineAsync("250-8BITMIME");
				await _smtpStream.WriteLineAsync("250-PIPELINING");
				await _smtpStream.WriteLineAsync("250 Ok");
			}

			return true;
		}

		private async Task<bool> MailFromCommandAsync(string command)
		{
			if (!command.StartsWith("MAIL FROM:", StringComparison.OrdinalIgnoreCase))
				return false;

			_mailTransaction = _handlerFactory.GetTransaction();

			// Check for the 8BITMIME body parameter
			int bodyParaIndex = command.IndexOf(" BODY=", StringComparison.OrdinalIgnoreCase);
			string mimeMode = "";

			if (bodyParaIndex > -1)
			{
				// The body parameter was passed in.
				// Extract the mime mode, if it isn't reconised inform the client of invalid syntax.
				mimeMode = command.Substring(bodyParaIndex + " BODY=".Length).Trim();
				command = command.Substring(0, bodyParaIndex);

				if (mimeMode.Equals("7BIT", StringComparison.OrdinalIgnoreCase))
				{
					_mailTransaction.TransportMIME = SmtpTransportMIME._7BitASCII;
				}
				else if (mimeMode.Equals("8BITMIME", StringComparison.OrdinalIgnoreCase))
				{
					_mailTransaction.TransportMIME = SmtpTransportMIME._8BitUTF;
				}
				else
				{
					await _smtpStream.WriteLineAsync("501 Syntax error");
					return true;
				}
			}

			string mailFrom = string.Empty;
			try
			{
				string address = command.Substring(command.IndexOf(":") + 1);
				if (address.Trim().Equals("<>"))
					mailFrom = null;
				else
					mailFrom = new System.Net.Mail.MailAddress(address).Address;
			}
			catch (Exception)
			{
				// Mail from not valid email.
				_smtpStream.WriteLine("501 Syntax error");
				return true;
			}

			// If we got this far mail from has an valid email address parameter so set it in the transaction
			// and return success to the client.
			_mailTransaction.MailFrom = mailFrom;
			await _smtpStream.WriteLineAsync("250 Ok");

			return true;
		}

		private async Task<bool> RcptCommandAsync(string command)
		{
			if (!command.StartsWith("RCPT TO:", StringComparison.OrdinalIgnoreCase))
				return false;

			// Check we have a Mail From address.
			if (_mailTransaction?.HasMailFrom == false)
			{
				await _smtpStream.WriteLineAsync("503 Bad sequence of commands");
				return true;
			}

			// Check that the RCPT TO has a valid email address parameter.
			MailAddress rcptTo = null;
			try
			{
				rcptTo = new MailAddress(command.Substring(command.IndexOf(":") + 1));
			}
			catch (Exception)
			{
				// Mail from not valid email.
				_smtpStream.WriteLine("501 Syntax error");
				return true;
			}

			// Check to see if mail is to be delivered locally or relayed for delivery somewhere else.
			if (_config.LocalDomains.Count(ld => ld.Hostname.Equals(rcptTo.Host, StringComparison.OrdinalIgnoreCase)) < 1)
			{
				// Messages isn't for delivery on this server.
				// Check if we are allowed to relay for the client IP
				if (!_config.IPsToAllowRelaying.Contains(_smtpStream.RemoteAddress.ToString()))
				{
					// This server cannot deliver or relay message for the MAIL FROM + RCPT TO addresses.
					// This should be treated as a permament failer, tell client not to retry.
					await _smtpStream.WriteLineAsync("554 Cannot relay");
					return true;
				}

				// Message is for relaying.
				_mailTransaction.MessageDestination = MessageDestination.Relay;
			}
			else
			{
				// Message to be delivered locally. Make sure mailbox is abuse/postmaster or feedback loop.
				if (!rcptTo.User.Equals("abuse", StringComparison.OrdinalIgnoreCase) &&
					!rcptTo.User.Equals("postmaster", StringComparison.OrdinalIgnoreCase) &&
					!rcptTo.User.StartsWith("return-", StringComparison.OrdinalIgnoreCase) &&
					!_feedbackDb.IsFeedbackLoopEmailAddress(rcptTo.Address))
				{
					await _smtpStream.WriteLineAsync("550 Unknown mailbox");
					return true;
				}

				_mailTransaction.MessageDestination = MessageDestination.Self;
			}

			// Add the recipient.
			_mailTransaction.RcptTo.Add(rcptTo.ToString());
			await _smtpStream.WriteLineAsync("250 Ok");
			return true;
		}

		private async Task<bool> DataCommandAsync(string command)
		{
			if (!command.Equals("DATA", StringComparison.OrdinalIgnoreCase))
				return false;

			// Must have a MAIL FROM before data.
			if (_mailTransaction?.HasMailFrom == false)
			{
				await _smtpStream.WriteLineAsync("503 Bad sequence of commands");
				return true;
			}

			// Must have RCPT's before data.
			if (_mailTransaction.RcptTo.Count < 1)
			{
				await _smtpStream.WriteLineAsync("554 No valid recipients");
				return true;
			}

			// Tell the client we are now accepting there data.
			await _smtpStream.WriteLineAsync("354 Go ahead");

			// Set the transport MIME to default or as specified by mail from body
			_smtpStream.SetSmtpTransportMIME(_mailTransaction.TransportMIME);

			// Wait for the first data line. Don't log data in SMTP log file.
			string dataline = await _smtpStream.ReadLineAsync(false);
			StringBuilder dataBuilder = new StringBuilder();
			// Loop until data client stops sending us data.
			while (!dataline.Equals("."))
			{
				// Add the line to existing data.
				dataBuilder.AppendLine(dataline);

				// Wait for the next data line. Don't log data in SMTP log file.
				dataline = await _smtpStream.ReadLineAsync(false);
			}
			_mailTransaction.Data = dataBuilder.ToString();

			// Data has been received, return to 7 bit ascii.
			_smtpStream.SetSmtpTransportMIME(SmtpTransportMIME._7BitASCII);

			// Once data is finished we have mail for delivery or relaying.
			// Add the Received header.
			_mailTransaction.AddHeader("Received", string.Format("from {0}[{1}] by {2}[{3}] on {4}",
				_host,
				_smtpStream.RemoteAddress.ToString(),
				_serverHostname,
				_smtpStream.LocalAddress.ToString(),
				DateTimeOffset.UtcNow.ToString("ddd, dd MMM yyyy HH':'mm':'ss -0000 (UTC)")));

			// Complete the transaction,either saving to local mailbox or queueing for relay.
			SmtpServerTransactionAsyncResult result = await _mailTransaction.SaveAsync();

			// Send a response to the client depending on the result of saving the transaction.
			switch (result)
			{
				case SmtpServerTransactionAsyncResult.SuccessMessageDelivered:
				case SmtpServerTransactionAsyncResult.SuccessMessageQueued:
					await _smtpStream.WriteLineAsync("250 Message queued for delivery");
					break;

				case SmtpServerTransactionAsyncResult.FailedSendDiscarding:
					await _smtpStream.WriteLineAsync("554 Send Discarding.");
					break;

				case SmtpServerTransactionAsyncResult.FailedToEnqueue:
					await _smtpStream.WriteLineAsync("421 Service unavailable");
					break;

				case SmtpServerTransactionAsyncResult.Unknown:
				default:
					await _smtpStream.WriteLineAsync("451 Requested action aborted: local error in processing.");
					break;
			}

			// Done with transaction, clear it and inform client message success and QUEUED
			_mailTransaction = null;

			// Go and wait for the next client command.
			return true;
		}
	}
}