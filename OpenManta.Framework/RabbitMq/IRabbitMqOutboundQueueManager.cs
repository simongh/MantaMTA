using System.Collections.Generic;
using System.Threading.Tasks;
using OpenManta.Core;

namespace OpenManta.Framework.RabbitMq
{
	internal interface IRabbitMqOutboundQueueManager
	{
		Task<MtaQueuedMessage> Dequeue();

		void Enqueue(IList<MtaMessage> inboundMessages);

		Task<bool> Enqueue(MtaQueuedMessage msg);

		void Ack(MtaQueuedMessage msg);
	}
}