using System;
using System.Collections.Generic;
using OpenManta.Core;

namespace OpenManta.Framework
{
	public interface IMtaParameters
	{
		IEnumerable<int> ServerListeningPorts { get; }
		string MTA_DROPFOLDER { get; }
		string AbuseDropFolder { get; }
		string BounceDropFolder { get; }
		string FeedbackLoopDropFolder { get; }
		string PostmasterDropFolder { get; }
		string MTA_QUEUEFOLDER { get; }
		string MTA_SMTP_LOGFOLDER { get; }
		IList<LocalDomain> LocalDomains { get; }
		string ReturnPathDomain { get; }
		string[] IPsToAllowRelaying { get; }
		int MtaRetryInterval { get; }
		int MtaMaxTimeInQueue { get; }
		bool KeepBounceFiles { get; }
		int DaysToKeepSmtpLogsFor { get; }
		Uri EventForwardingHttpPostUrl { get; }

		Client Client { get; }

		RabbitMQ RabbitMq { get; }
	}
}