using System.Collections.Generic;

namespace OpenManta.Data
{
	public interface ICfgPara
	{
		IEnumerable<int> ServerListenPorts { get; }

		string DropFolder { get; }

		string QueueFolder { get; }

		string LogFolder { get; }

		int RetryIntervalBaseMinutes { get; set; }

		int MaxTimeInQueueMinutes { get; set; }

		int DefaultVirtualMtaGroupID { get; set; }

		int ClientIdleTimeout { get; set; }

		int DaysToKeepSmtpLogsFor { get; set; }

		int ReceiveTimeout { get; set; }

		int ReturnPathDomainId { get; set; }

		int SendTimeout { get; set; }

		string EventForwardingHttpPostUrl { get; set; }

		bool KeepBounceFilesFlag { get; }

		bool RabbitMqEnabled { get; }

		string RabbitMqUsername { get; }

		string RabbitMqPassword { get; }

		string RabbitMqHostname { get; }
	}
}