using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenManta.Core;

namespace OpenManta.Framework.Queues
{
	internal class InMemoryInboundQueueManager : IInboundQueueManager
	{
		public void Ack(ulong deliveryTag, bool multiple)
		{
		}

		public Task<IList<MtaMessage>> Dequeue(int maxItems)
		{
			IList<MtaMessage> result = new List<MtaMessage>();
			return Task.FromResult(result);
		}

		public Task<bool> Enqueue(Guid messageID, int ipGroupID, int internalSendID, string mailFrom, string[] rcptTo, string message, MessagePriority priority)
		{
			return Task.FromResult(true);
		}
	}
}