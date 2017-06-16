using System.Collections.Generic;
using System.Threading.Tasks;
using OpenManta.Core;

namespace OpenManta.Framework.Queues
{
	public interface IOutboundQueueManager
	{
		Task<MtaQueuedMessage> Dequeue();

		void Enqueue(IList<MtaMessage> inboundMessages);

		Task<bool> Enqueue(MtaQueuedMessage msg);

		void Ack(MtaQueuedMessage msg);
	}
}