using OpenManta.Core;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenManta.Framework.RabbitMq
{
	internal class RabbitMqOutboundQueueManager : Queues.IOutboundQueueManager
	{
		private readonly IRabbitMqManager _manager;

		public RabbitMqOutboundQueueManager(IRabbitMqManager manager)
		{
			Guard.NotNull(manager, nameof(manager));

			_manager = manager;
		}

		/// <summary>
		/// Dequeue a message from RabbitMQ.
		/// </summary>
		/// <returns>A dequeued message or null if there weren't any.</returns>
		public async Task<MtaQueuedMessage> Dequeue()
		{
			BasicDeliverEventArgs ea = null;
			try
			{
				ea = _manager.Dequeue(RabbitMqQueue.OutboundWaiting, 1, 100).FirstOrDefault();
			}
			catch (Exception)
			{
				ea = null;
			}
			if (ea == null)
				return null;

			MtaQueuedMessage qmsg = await Serialisation.Deserialise<MtaQueuedMessage>(ea.Body);
			qmsg.RabbitMqDeliveryTag = ea.DeliveryTag;
			qmsg.IsHandled = false;
			return qmsg;
		}

		/// <summary>
		/// Enqueue the messages in the collection for relaying.
		/// </summary>
		/// <param name="inboundMessages">Messages to enqueue.</param>
		public void Enqueue(IList<MtaMessage> inboundMessages)
		{
			Parallel.ForEach(inboundMessages, message =>
			{
				Enqueue(MtaQueuedMessage.CreateNew(message)).GetAwaiter().GetResult();
			});

			_manager.Ack(RabbitMqQueue.Inbound, inboundMessages.Max(m => m.RabbitMqDeliveryTag), true);
		}

		/// <summary>
		/// Enqueue the message for relaying.
		/// </summary>
		/// <param name="msg">Message to enqueue.</param>
		public async Task<bool> Enqueue(MtaQueuedMessage msg)
		{
			Guard.NotNull(msg, nameof(msg));

			RabbitMqQueue queue = RabbitMqQueue.OutboundWaiting;

			int secondsUntilNextAttempt = (int)Math.Ceiling((msg.AttemptSendAfterUtc - DateTimeOffset.UtcNow).TotalSeconds);

			if (secondsUntilNextAttempt > 0)
			{
				if (secondsUntilNextAttempt < 10)
					queue = RabbitMqQueue.OutboundWait1;
				else if (secondsUntilNextAttempt < 60)
					queue = RabbitMqQueue.OutboundWait10;
				else if (secondsUntilNextAttempt < 300)
					queue = RabbitMqQueue.OutboundWait60;
				else
					queue = RabbitMqQueue.OutboundWait300;
			}

			var published = await _manager.Publish(msg, queue, priority: msg.RabbitMqPriority);

			if (published)
				msg.IsHandled = true;

			return published;
		}

		/// <summary>
		/// Acknowledge the message as handled.
		/// </summary>
		/// <param name="msg">The message to acknowledge.</param>
		public void Ack(MtaQueuedMessage msg)
		{
			Guard.NotNull(msg, nameof(msg));

			_manager.Ack(RabbitMqQueue.OutboundWaiting, msg.RabbitMqDeliveryTag, false);
		}
	}
}