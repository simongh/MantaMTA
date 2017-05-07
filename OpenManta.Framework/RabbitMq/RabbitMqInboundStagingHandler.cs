using System.Linq;
using System.Threading.Tasks;
using OpenManta.Core;
using RabbitMQ.Client.Events;

namespace OpenManta.Framework.RabbitMq
{
	internal class RabbitMqInboundStagingHandler : IRabbitMqInboundStagingHandler, IStopRequired
	{
		private const int STAGING_DEQUEUE_TASKS = 25;
		public int _StartedThreads = 0;
		private bool IsStopping = false;
		private readonly IRabbitMqManager _manager;

		public RabbitMqInboundStagingHandler(IMantaCoreEvents coreEvents, IRabbitMqManager manager)
		{
			Guard.NotNull(coreEvents, nameof(coreEvents));
			Guard.NotNull(manager, nameof(manager));

			_manager = manager;

			coreEvents.RegisterStopRequiredInstance(this);
		}

		public void Start()
		{
			Parallel.For(0, STAGING_DEQUEUE_TASKS, (i) =>
			{
				var t = new System.Threading.Thread(new System.Threading.ThreadStart(HandleDequeue));
				t.Start();
			});
		}

		public void Stop()
		{
			IsStopping = true;
		}

		private void HandleDequeue()
		{
			while (!IsStopping)
			{
				BasicDeliverEventArgs ea = _manager.Dequeue(RabbitMqManager.RabbitMqQueue.InboundStaging, 1, 100).FirstOrDefault();
				if (ea == null)
				{
					//await Task.Delay(1000);
					System.Threading.Thread.Sleep(1000);
					continue;
				}

				MtaQueuedMessage qmsg = Serialisation.Deserialise<MtaQueuedMessage>(ea.Body).Result;
				MtaMessage msg = new MtaMessage
				{
					ID = qmsg.ID,
					InternalSendID = qmsg.InternalSendID,
					MailFrom = qmsg.MailFrom,
					RcptTo = qmsg.RcptTo,
					VirtualMTAGroupID = qmsg.VirtualMTAGroupID
				};

				_manager.Publish(msg, RabbitMqManager.RabbitMqQueue.Inbound, true, qmsg.RabbitMqPriority).Wait();
				_manager.Publish(qmsg, RabbitMqManager.RabbitMqQueue.OutboundWaiting, true, qmsg.RabbitMqPriority).Wait();
				_manager.Ack(RabbitMqManager.RabbitMqQueue.InboundStaging, ea.DeliveryTag, false);
			}
		}
	}
}