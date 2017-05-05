using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenManta.Core;

namespace OpenManta.Framework
{
	public interface IQueueManager
	{
		Task<bool> Enqueue(Guid messageID, int ipGroupID, int internalSendID, string mailFrom, string[] rcptTo, string message, RabbitMqPriority priority);

		void Start();
	}
}