using System;
using System.Collections.Generic;
using log4net;
using OpenManta.Core;
using OpenManta.Framework;
using OpenManta.Framework.Queues;

namespace OpenManta.Service
{
	public class OpenMantaService
	{
		// Array will hold all instances of SmtpServer, one for each port we will be listening on.
		private readonly IList<ISmtpServer> SmtpServers;

		private readonly ILog _logging;
		private readonly IStagingHandler _handler;
		private readonly IQueueManager _queues;
		private readonly IVirtualMtaManager _virtualMtas;
		private readonly IMtaParameters _config;
		private readonly ISmtpServerFactory _factory;
		private readonly IMessageSender _sender;
		private readonly IMantaCoreEvents _coreEvents;
		private readonly IEventsFileHandler _eventHandler;

		public OpenMantaService(ILog logging, IStagingHandler handler, IQueueManager queues, IVirtualMtaManager virtualMtas, IMtaParameters config, ISmtpServerFactory factory, IMessageSender sender, IMantaCoreEvents coreEvents, IEventsFileHandler eventHandler)
		{
			Guard.NotNull(logging, nameof(logging));
			Guard.NotNull(handler, nameof(handler));
			Guard.NotNull(queues, nameof(queues));
			Guard.NotNull(virtualMtas, nameof(virtualMtas));
			Guard.NotNull(config, nameof(config));
			Guard.NotNull(factory, nameof(factory));
			Guard.NotNull(sender, nameof(sender));
			Guard.NotNull(coreEvents, nameof(coreEvents));
			Guard.NotNull(eventHandler, nameof(eventHandler));

			_logging = logging;
			_handler = handler;
			_queues = queues;
			_virtualMtas = virtualMtas;
			_config = config;
			_factory = factory;
			_sender = sender;
			_coreEvents = coreEvents;
			_eventHandler = eventHandler;

			SmtpServers = new List<ISmtpServer>();
		}

		public void Start()
		{
			_logging.Info("Starting OpenManta Service.");
			AppDomain.CurrentDomain.UnhandledException += delegate (object sender, UnhandledExceptionEventArgs e)
			{
				Exception ex = (Exception)e.ExceptionObject;
				_logging.Fatal(ex.Message, ex);
			};

			_handler.Start();

			// Start the RabbitMQ Bulk inserter.
			_queues.Start();

			// Create the SmtpServers
			foreach (var vmta in _virtualMtas.GetVirtualMtasForListeningOn())
			{
				foreach (var port in _config.ServerListeningPorts)
				{
					var server = _factory.Create();
					SmtpServers.Add(server);

					server.Open(vmta.IPAddress, port);
				}
			}

			// Start the SMTP Client.
			_sender.Start();

			// Start the events (bounce/abuse) handler.
			_eventHandler.Start();

			_logging.Info("OpenManta Service has started.");
		}

		public void Stop()
		{
			_logging.Info("Stopping OpenManta Service");

			// Need to wait while servers & client shutdown.
			_coreEvents.InvokeMantaCoreStopping();
			foreach (var smtp in SmtpServers)
				smtp.Dispose();

			_logging.Info("OpenManta Service has stopped.");
		}
	}
}