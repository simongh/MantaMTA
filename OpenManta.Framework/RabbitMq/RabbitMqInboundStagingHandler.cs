using OpenManta.Core;
using RabbitMQ.Client.Events;
using System.Linq;
using System.Threading.Tasks;

namespace OpenManta.Framework.RabbitMq
{
    public class RabbitMqInboundStagingHandler : IStopRequired
	{
        private const int STAGING_DEQUEUE_TASKS = 25;
        public int _StartedThreads = 0;
        private static RabbitMqInboundStagingHandler _Instance = new RabbitMqInboundStagingHandler();
        private bool IsStopping = false;
        private RabbitMqInboundStagingHandler()
        {
            MantaCoreEvents.RegisterStopRequiredInstance(this);
        }

        public static RabbitMqInboundStagingHandler Instance { get { return _Instance; } }
        public void Start()
		{
            Parallel.For(0, STAGING_DEQUEUE_TASKS, (i) => {
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
			while(!IsStopping)
			{
				BasicDeliverEventArgs ea = RabbitMq.RabbitMqManager.Dequeue(RabbitMqManager.RabbitMqQueue.InboundStaging, 1, 100).FirstOrDefault();
				if(ea == null)
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

                RabbitMqManager.Publish(msg, RabbitMqManager.RabbitMqQueue.Inbound, true, qmsg.RabbitMqPriority).Wait();
                RabbitMqManager.Publish(qmsg, RabbitMqManager.RabbitMqQueue.OutboundWaiting, true, qmsg.RabbitMqPriority).Wait();
				RabbitMqManager.Ack(RabbitMqManager.RabbitMqQueue.InboundStaging, ea.DeliveryTag, false);
			}
		}
    }
}
