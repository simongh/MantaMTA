using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace OpenManta.Core
{
	public class VirtualMTA : BaseEntity
	{
		/// <summary>
		/// The Hostname of specified for this IP Address.
		/// </summary>
		public string Hostname { get; set; }

		/// <summary>
		/// The .Net IP Address object for this Virtual MTA.
		/// </summary>
		public IPAddress IPAddress {get;set;} 

		/// <summary>
		/// If true the IP address can be used for receiving. 
		/// </summary>
		public bool IsSmtpInbound { get; set; }

		/// <summary>
		/// If true the IP address can be used for sending.
		/// </summary>
		public bool IsSmtpOutbound { get; set; }

		/// <summary>
		/// Holds the information regarding how often this IP has been used. 
		/// This is used for load balancing the IPs in a pool, it is never saved to the database.
		/// string = mx record hostname (lowercase).
		/// int = count of times used.
		/// </summary>
		internal ConcurrentDictionary<string, int> SendsCounter = new ConcurrentDictionary<string, int>();

		/// <summary>
		/// Creates a new TcpClient for this Virtual MTA.
		/// </summary>
		/// <param name="port"></param>
		/// <returns></returns>
		public TcpClient CreateTcpClient(int port)
		{
			return new TcpClient(new IPEndPoint(this.IPAddress, port));
		}

		/// <summary>
		/// Constructor sets defaults.
		/// </summary>
		public VirtualMTA()
		{
			this.IsSmtpInbound = true;
			this.IsSmtpOutbound = true;
		}
	}
}

