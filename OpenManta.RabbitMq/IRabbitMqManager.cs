using System.Collections.Generic;
using System.Threading.Tasks;
using OpenManta.Core;
using RabbitMQ.Client.Events;
using static OpenManta.Framework.RabbitMq.RabbitMqManager;

namespace OpenManta.Framework.RabbitMq
{
	internal interface IRabbitMqManager
	{
		void Ack(RabbitMqQueue queue, ulong deliveryTag, bool multiple);

		IList<BasicDeliverEventArgs> Dequeue(RabbitMqQueue queue, int maxItems, int millisecondsTimeout);

		PublishChannel GetPublishChannel(RabbitMqQueue queue, bool noConfirm);

		bool Publish(byte[] message, RabbitMqQueue queue, bool noConfirm, MessagePriority priority);

		Task<bool> Publish(object obj, RabbitMqQueue queue, bool confirm = true, MessagePriority priority = MessagePriority.Low);
	}
}