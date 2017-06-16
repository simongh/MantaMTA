using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenManta.Core;

namespace OpenManta.Framework.Queues
{
	public interface IInboundQueueManager
	{
		Task<IList<MtaMessage>> Dequeue(int maxItems);

		Task<bool> Enqueue(Guid messageID, int ipGroupID, int internalSendID, string mailFrom, string[] rcptTo, string message, MessagePriority priority);

		void Ack(ulong deliveryTag, bool multiple);
	}
}