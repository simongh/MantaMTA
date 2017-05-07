using OpenManta.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;

namespace OpenManta.Framework.RabbitMq
{
	internal class RabbitMqInboundQueueManager : IRabbitMqInboundQueueManager
	{
		private readonly IRabbitMqManager _manager;

		public RabbitMqInboundQueueManager(IRabbitMqManager manager)
		{
			Guard.NotNull(manager, nameof(manager));

			_manager = manager;
		}

		/// <summary>
		/// Dequeues a collection of inbound messages from RabbitMQ.
		/// </summary>
		/// <param name="maxItems">The maximum amount of messages to dequeue.</param>
		/// <returns>The dequeue messages.</returns>
		public async Task<IList<MtaMessage>> Dequeue(int maxItems)
		{
			IList<BasicDeliverEventArgs> items = _manager.Dequeue(RabbitMqManager.RabbitMqQueue.Inbound, maxItems, 1 * 1000);
			IList<MtaMessage> messages = new List<MtaMessage>();
			if (items.Count == 0)
				return messages;

			foreach (BasicDeliverEventArgs ea in items)
			{
				MtaMessage msg = await Serialisation.Deserialise<MtaMessage>(ea.Body);
				msg.RabbitMqDeliveryTag = ea.DeliveryTag;
				messages.Add(msg);
			}

			return messages;
		}

		/// <summary>
		/// Enqueues the Email that we are going to relay in RabbitMQ.
		/// </summary>
		/// <param name="messageID">ID of the Message being Queued.</param>
		/// <param name="ipGroupID">ID of the Virtual MTA Group to send the Message through.</param>
		/// <param name="internalSendID">ID of the Send the Message is apart of.</param>
		/// <param name="mailFrom">The envelope mailfrom, should be return-path in most instances.</param>
		/// <param name="rcptTo">The envelope rcpt to.</param>
		/// <param name="message">The Email.</param>
		/// <param name="priority">Priority of message.</param>
		/// <returns>True if the Email has been enqueued in RabbitMQ.</returns>
		public async Task<bool> Enqueue(Guid messageID, int ipGroupID, int internalSendID, string mailFrom, string[] rcptTo, string message, RabbitMqPriority priority)
		{
			// Create the thing we are going to queue in RabbitMQ.
			var recordToSave = new MtaMessage
			{
				ID = messageID,
				InternalSendID = internalSendID,
				MailFrom = mailFrom,
				Message = message,
				RcptTo = rcptTo,
				VirtualMTAGroupID = ipGroupID,
				RabbitMqPriority = priority
			};

			return await _manager.Publish(MtaQueuedMessage.CreateNew(recordToSave), RabbitMqManager.RabbitMqQueue.InboundStaging, true, priority);
		}
	}
}