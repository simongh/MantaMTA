﻿using System;
using System.Threading.Tasks;
using OpenManta.Core;

namespace OpenManta.Framework
{
	public interface IQueueManager
	{
		Task<bool> Enqueue(Guid messageID, int ipGroupID, int internalSendID, string mailFrom, string[] rcptTo, string message, MessagePriority priority);

		void Start();
	}
}