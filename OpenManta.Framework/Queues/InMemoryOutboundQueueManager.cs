using System.Collections.Generic;
using System.Threading.Tasks;
using OpenManta.Core;

namespace OpenManta.Framework.Queues
{
	internal class InMemoryOutboundQueueManager : IOutboundQueueManager
	{
		public void Ack(MtaQueuedMessage msg)
		{
		}

		public Task<MtaQueuedMessage> Dequeue()
		{
			return Task.FromResult<MtaQueuedMessage>(null);
		}

		public void Enqueue(IList<MtaMessage> inboundMessages)
		{
		}

		public Task<bool> Enqueue(MtaQueuedMessage msg)
		{
			return Task.FromResult(true);
		}
	}
}