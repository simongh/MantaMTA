﻿using OpenManta.Core;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OpenManta.Framework.Smtp
{
	/// <summary>
	/// Makes it easy to work with TCP SMTP connections.
	/// </summary>
	internal class SmtpStreamHandler : ISmtpStreamHandler
	{
		/// <summary>
		/// Holds a Copy of a UTF8 Encoding without a BOM.
		/// </summary>
		private static Encoding _UTF8Encoding = new UTF8Encoding(false);

		/// <summary>
		/// The SMTP Transport MIME currently set.
		/// </summary>
		private SmtpTransportMIME _CurrentTransportMIME;

		/// <summary>
		/// Stream reader for the underlying connection. Encoding is 7bit ASCII.
		/// </summary>
		private StreamReader ClientStreamReaderASCII;

		/// <summary>
		/// Stream reader for the underlying connection. Encoding is UTF8.
		/// </summary>
		private StreamReader ClientStreamReaderUTF8;

		/// <summary>
		/// Stream writer for the underlying connection. Encoding is 7bit ASCII.
		/// </summary>
		private StreamWriter ClientStreamWriterASCII;

		/// <summary>
		/// Stream writer for the underlying connection. Encoding is UTF8.
		/// </summary>
		private StreamWriter ClientStreamWriterUTF8;

		/// <summary>
		/// The port number connected to at the local address.
		/// </summary>
		private int LocalPort;

		/// <summary>
		/// The port number connected to at the remote address.
		/// </summary>
		private int RemotePort;

		private readonly ISmtpTransactionLogger _logger;

		public SmtpStreamHandler(ISmtpTransactionLogger logger)
		{
			Guard.NotNull(logger, nameof(logger));

			_logger = logger;
		}

		/// <summary>
		/// Create an SmtpStreamHandler from the TCP client.
		/// </summary>
		/// <param name="client"></param>
		public void Open(TcpClient client)
		{
			Open(client.GetStream());

			IPEndPoint remote = client.Client.RemoteEndPoint as IPEndPoint;
			this.RemoteAddress = remote.Address;
			this.RemotePort = remote.Port;

			IPEndPoint local = client.Client.LocalEndPoint as IPEndPoint;
			this.LocalAddress = local.Address;
			this.LocalPort = local.Port;
		}

		/// <summary>
		/// Constructor is used for NUnit tests and SmtpStreamHandler(TcpClient).
		/// </summary>
		/// <param name="stream"></param>
		public void Open(Stream stream)
		{
			this._CurrentTransportMIME = SmtpTransportMIME._7BitASCII;

			// Use new UTF8Encoding(false) so we don't send BOM to the network stream.
			this.ClientStreamReaderUTF8 = new StreamReader(stream, _UTF8Encoding);
			this.ClientStreamWriterUTF8 = new StreamWriter(stream, _UTF8Encoding);
			this.ClientStreamReaderASCII = new StreamReader(stream, Encoding.ASCII);
			this.ClientStreamWriterASCII = new StreamWriter(stream, Encoding.ASCII);
		}

		/// <summary>
		/// The local address is the address on the server that the client is connected to.
		/// </summary>
		public IPAddress LocalAddress { get; set; }

		/// <summary>
		/// The remote address is the source of the client request.
		/// </summary>
		public IPAddress RemoteAddress { get; set; }

		/// <summary>
		/// Reads all lines from the stream.
		/// </summary>
		/// <param name="log">If true will log.</param>
		/// <returns>All lines from the stream that are considered part of one message by SMTP.</returns>
		public string ReadAllLines(bool log = true)
		{
			return ReadAllLinesAsync(log).Result;
		}

		/// <summary>
		/// Reads all lines from the stream.
		/// </summary>
		/// <param name="log">If true will log.</param>
		/// <returns>All lines from the stream that are considered part of one message by SMTP.</returns>
		public async Task<string> ReadAllLinesAsync(bool log = true)
		{
			StringBuilder sb = new StringBuilder();

			string line = string.Empty;
			try
			{
				line = await ReadLineAsync(false);
			}
			catch (Exception)
			{
				return "421 Connection ended abruptly";
			}

			while (line[3] == '-')
			{
				sb.AppendLine(line);
				try
				{
					line = await ReadLineAsync(false);
				}
				catch (Exception)
				{
					if (sb.Length == 0)
						return "421 Connection ended abruptly";
					line = string.Empty;
					break;
				}
			}
			sb.AppendLine(line);

			string result = sb.ToString();

			if (log)
				LogSmtpConversationMessage(SmtpConversationDirection.INBOUND, result);

			return result;
		}

		/// <summary>
		/// Read an SMTP line from the client.
		/// </summary>
		/// <param name="log">If true will log.</param>
		/// <returns>Line read from the stream.</returns>
		public async Task<string> ReadLineAsync(bool log = true)
		{
			string response = string.Empty;

			// Read the underlying stream using the correct encoding.
			if (_CurrentTransportMIME == SmtpTransportMIME._7BitASCII)
				response = await ClientStreamReaderASCII.ReadLineAsync();
			else if (_CurrentTransportMIME == SmtpTransportMIME._8BitUTF)
				response = await ClientStreamReaderUTF8.ReadLineAsync();
			else
				throw new NotImplementedException(_CurrentTransportMIME.ToString());

			if (response == null)
				throw new IOException("Remote Endpoint Disconnected.");

			if (log)
				LogSmtpConversationMessage(SmtpConversationDirection.INBOUND, response);

			return response;
		}

		/// <summary>
		/// Set MIME type to be used for reading/writing the underlying stream.
		/// </summary>
		/// <param name="mime">Transport MIME to begin using.</param>
		public void SetSmtpTransportMIME(SmtpTransportMIME mime)
		{
			_CurrentTransportMIME = mime;
		}

		/// <summary>
		/// Write a line to the Stream. Using the current transport MIME.
		/// </summary>
		/// <param name="message">Message to send.</param>
		/// <param name="log">If true will log.</param>
		public void WriteLine(string message, bool log = true)
		{
			WriteLineAsync(message, log).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Write a line to the Stream. Using the current transport MIME.
		/// </summary>
		/// <param name="message">Message to send.</param>
		/// <param name="log">If true will log.</param>
		public async Task<bool> WriteLineAsync(string message, bool log = true)
		{
			if (_CurrentTransportMIME == SmtpTransportMIME._7BitASCII)
			{
				try
				{
					await ClientStreamWriterASCII.WriteLineAsync(message);
					await ClientStreamWriterASCII.FlushAsync();
				}
				catch (Exception)
				{
					return false;
				}
			}
			else if (_CurrentTransportMIME == SmtpTransportMIME._8BitUTF)
			{
				try
				{
					await ClientStreamWriterUTF8.WriteLineAsync(message);
					await ClientStreamWriterUTF8.FlushAsync();
				}
				catch (Exception)
				{
					return false;
				}
			}
			else
				throw new NotImplementedException(_CurrentTransportMIME.ToString());

			if (log)
				LogSmtpConversationMessage(SmtpConversationDirection.OUTBOUND, message);

			return true;
		}

		/// <summary>
		/// Write to the Stream. Using the current transport MIME.
		/// </summary>
		/// <param name="message">Message to send.</param>
		/// <param name="log">If true will log.</param>
		public async Task<bool> WriteAsync(string message, bool log = true)
		{
			if (_CurrentTransportMIME == SmtpTransportMIME._7BitASCII)
			{
				try
				{
					await ClientStreamWriterASCII.WriteAsync(message);
					await ClientStreamWriterASCII.FlushAsync();
				}
				catch (Exception)
				{
					return false;
				}
			}
			else if (_CurrentTransportMIME == SmtpTransportMIME._8BitUTF)
			{
				try
				{
					await ClientStreamWriterUTF8.WriteAsync(message);
					await ClientStreamWriterUTF8.FlushAsync();
				}
				catch (Exception)
				{
					return false;
				}
			}
			else
				throw new NotImplementedException(_CurrentTransportMIME.ToString());

			if (log)
				LogSmtpConversationMessage(SmtpConversationDirection.OUTBOUND, message);

			return true;
		}

		/// <summary>
		/// Log Smtp conversation message.
		/// </summary>
		/// <param name="direction">Direction the message went, either Inbound or Outbound.</param>
		/// <param name="message">The smtp message.</param>
		private void LogSmtpConversationMessage(string direction, string message)
		{
			_logger.Log(", " + this.LocalAddress + ":" + this.LocalPort + ", " + this.RemoteAddress + ":" + this.RemotePort + ", " + direction + ", " + message);
		}

		/// <summary>
		///	Logging directions for SMTP conversations.
		/// </summary>
		private struct SmtpConversationDirection
		{
			/// <summary>
			/// This server received the message.
			/// </summary>
			public const string INBOUND = "Inbound";

			/// <summary>
			/// This server sent the message.
			/// </summary>
			public const string OUTBOUND = "Outbound";
		}
	}
}