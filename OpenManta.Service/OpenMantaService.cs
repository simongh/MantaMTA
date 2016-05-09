using OpenManta.Framework;
using OpenManta.Framework.RabbitMq;
using System;
using System.Collections.Generic;
using System.ServiceProcess;

namespace OpenManta.Service
{
    public partial class OpenMantaService : ServiceBase
	{
		// Array will hold all instances of SmtpServer, one for each port we will be listening on.
		private readonly IList<SmtpServer> SmtpServers = new List<SmtpServer>();

		public OpenMantaService()
		{
			InitializeComponent();
		}

		protected override void OnStart(string[] args)
		{
            Logging.Info("Starting OpenManta Service.");
			AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
			{
				Exception ex = (Exception)e.ExceptionObject;
				Logging.Fatal(ex.Message, ex);
			};

            RabbitMqInboundStagingHandler.Instance.Start();

            // Start the RabbitMQ Bulk inserter.
            QueueManager.Instance.Start();
            			
			// Create the SmtpServers
            foreach(var vmta in VirtualMtaManager.GetVirtualMtasForListeningOn())
			{
                foreach(var port in MtaParameters.ServerListeningPorts)
					SmtpServers.Add(new SmtpServer(vmta.IPAddress, port));
			}

			// Start the SMTP Client.
			MessageSender.Instance.Start();

			// Start the events (bounce/abuse) handler.
			EventsFileHandler.Instance.Start();

			Logging.Info("OpenManta Service has started.");
		}

		protected override void OnStop()
		{
			Logging.Info("Stopping OpenManta Service");

			// Need to wait while servers & client shutdown.
			MantaCoreEvents.InvokeMantaCoreStopping();
            foreach (var smtp in SmtpServers)
                smtp.Dispose();

			Logging.Info("OpenManta Service has stopped.");
		}
	}
}
