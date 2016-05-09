using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using OpenManta.Core;
using OpenManta.Framework;

namespace OpenManta.Console
{
	class Program
	{
		static void Main(string[] args)
		{
			Logging.Info("MTA Started");

			AppDomain.CurrentDomain.FirstChanceException += delegate(object sender, FirstChanceExceptionEventArgs e)
			{
				Logging.Warn("", e.Exception);
			};

			IList<VirtualMTA> ipAddresses = VirtualMtaManager.GetVirtualMtasForListeningOn();

			// Array will hold all instances of SmtpServer, one for each port we will be listening on.
			List<SmtpServer> smtpServers = new List<SmtpServer>();
			
			// Create the SmtpServers
			for (int c = 0; c < ipAddresses.Count; c++)
			{
				VirtualMTA ipAddress = ipAddresses[c];
				for (int i = 0; i < MtaParameters.ServerListeningPorts.Length; i++)
					smtpServers.Add(new SmtpServer(ipAddress.IPAddress, MtaParameters.ServerListeningPorts[i]));
			}

			// Start the SMTP Client.
			MessageSender.Instance.Start();
			// Start the events (bounce/abuse) handler.
			EventsFileHandler.Instance.Start();

			QueueManager.Instance.Start();
			OpenManta.Framework.RabbitMq.RabbitMqInboundStagingHandler.Instance.Start();

			bool quit = false;
			while (!quit)
			{
				ConsoleKeyInfo key = System.Console.ReadKey(true);
				if (key.KeyChar == 'q' || key.KeyChar == 'Q')
					quit = true;
			}

			// Need to wait while servers & client shutdown.
			MantaCoreEvents.InvokeMantaCoreStopping();
			foreach (SmtpServer s in smtpServers)
				s.Dispose ();

			Logging.Info("MTA Stopped");
			System.Console.WriteLine("Press any key to continue");
			System.Console.ReadKey(true);
		}
	}
}
