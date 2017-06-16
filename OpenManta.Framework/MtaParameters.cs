using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using Ninject;
using OpenManta.Core;
using OpenManta.Data;

namespace OpenManta.Framework
{
	public class MtaParameters : IMtaParameters, IInitializable
	{
		private readonly MemoryCache _cache;

		/// <summary>
		/// Name of the MTA. Used in welcome banner to identify product as well as email headers.  Don't use spaces or interesting characters.
		/// </summary>
		public const string MTA_NAME = "MantaMTA";

		/// <summary>
		/// New line as should be used in emails.
		/// </summary>
		public const string NewLine = "\r\n";

		/// <summary>
		/// The time in minutes of how long stuff should be cached in memory for.
		/// </summary>
		internal const int MTA_CACHE_MINUTES = 5;

		/// <summary>
		/// This is the ID of the outbound rule mx pattern that should be used as the default.
		/// </summary>
		public const int OUTBOUND_RULES_DEFAULT_PATTERN_ID = -1;

		/// <summary>
		/// The string Manta uses when a message could not be delivered within the <paramref name="MantaMTA.Core.MtaParameters.MtaMaxTimeInQueue"/> value.
		/// This used as the text to process to identify what happened with delivery.
		/// </summary>
		public const string TIMED_OUT_IN_QUEUE_MESSAGE = "Timed out in queue.";

		private readonly ICfgPara _config;
		private readonly ICfgLocalDomains _localDomainConfig;
		private readonly ICfgRelayingPermittedIP _permittedIpConfig;

		public MtaParameters(ICfgPara config, ICfgLocalDomains localDomainConfig, ICfgRelayingPermittedIP permittedIpConfig)
		{
			Guard.NotNull(config, nameof(config));
			Guard.NotNull(localDomainConfig, nameof(localDomainConfig));
			Guard.NotNull(permittedIpConfig, nameof(permittedIpConfig));

			_cache = MemoryCache.Default;
			_config = config;
			_localDomainConfig = localDomainConfig;
			_permittedIpConfig = permittedIpConfig;

			Client = new Client(config);
			RabbitMq = new RabbitMQ(config);
		}

		/// <summary>
		/// Gets the ports that the SMTP server should listen for client connections on.
		/// This will almost always be 25 & 587.
		/// </summary>
		public IEnumerable<int> ServerListeningPorts => _config.ServerListenPorts;

		/// <summary>
		/// Drop folder, for incoming messages.
		/// This should be in config.
		/// </summary>
		public string DropFolder => _config.DropFolder;

		/// <summary>
		/// Drop folder for abuse@
		/// </summary>
		public string AbuseDropFolder => Path.Combine(DropFolder, "abuse");

		/// <summary>
		/// Drop folder for email bounces.
		/// </summary>
		public string BounceDropFolder => Path.Combine(DropFolder, "bounce");

		/// <summary>
		/// Drop folder for feedback loop emails.
		/// </summary>
		public string FeedbackLoopDropFolder => Path.Combine(DropFolder, "feedback");

		/// <summary>
		/// Drop folder for postmaster@
		/// </summary>
		public string PostmasterDropFolder => Path.Combine(DropFolder, "postmaster");

		/// <summary>
		/// Queue folder, for messages to be sent.
		/// </summary>
		public string QueueFolder => _config.QueueFolder;

		/// <summary>
		/// Log foler, where SMTP Transaction logs will go.
		/// This should be in config.
		/// </summary>
		public string LogFolder => _config.LogFolder;

		/// <summary>
		/// List of domains to accept messages for drop folder.
		/// All domains are toLowered!
		/// </summary>
		public IList<LocalDomain> LocalDomains => _cache.GetValue("config_localdomains", () => _localDomainConfig.GetLocalDomainsArray());

		/// <summary>
		/// The domain that return paths should use.
		/// </summary>
		public string ReturnPathDomain => LocalDomains.Single(d => d.ID == _config.ReturnPathDomainId).Hostname;

		/// <summary>
		/// List of IP addresses to allow relaying for.
		/// </summary>
		public IEnumerable<string> IPsToAllowRelaying => _cache.GetValue("config_allowedIPs", () => _permittedIpConfig.GetRelayingPermittedIPAddresses());

		/// <summary>
		/// The time in minutes between send retries.
		/// </summary>
		public int MtaRetryInterval => _config.RetryIntervalBaseMinutes;

		/// <summary>
		/// The maximum time in minutes that a message can be in the queue.
		/// </summary>
		public int MtaMaxTimeInQueue => _config.MaxTimeInQueueMinutes;

		/// <summary>
		/// Flag to indicate whether to retain succesfully processed bounce email files.  Used to see how bounces have been processed so
		/// the processing code can be reviewed and Bounce Rules modified if necessary.
		///
		/// Files that result in an error when being processed are always kept.
		///
		/// If true, successfully processed bounce email files are kept in folders relating to how they were identified;
		/// if false, they are immediately deleted.
		/// </summary>
		public bool KeepBounceFiles => _config.KeepBounceFilesFlag;

		/// <summary>
		/// The amount of days to keep SMTP logs for before deleting them.
		/// </summary>
		public int DaysToKeepSmtpLogsFor => _config.DaysToKeepSmtpLogsFor;

		/// <summary>
		/// The URL to post Manta Events (abuse/bounce) to.
		/// </summary>
		public Uri EventForwardingHttpPostUrl
		{
			get
			{
				var value = _config.EventForwardingHttpPostUrl;

				if (string.IsNullOrEmpty(value))
					return null;

				return new Uri(value);
			}
		}

		public Client Client { get; private set; }

		public RabbitMQ RabbitMq { get; private set; }

		public void Initialize()
		{
			if (!Directory.Exists(DropFolder))
				Directory.CreateDirectory(DropFolder);

			if (!Directory.Exists(QueueFolder))
				Directory.CreateDirectory(QueueFolder);

			if (!Directory.Exists(LogFolder))
				Directory.CreateDirectory(LogFolder);
		}
	}

	public class Client
	{
		/// <summary>
		/// Port for SMTP connections by the client to remote servers when sending
		/// messages. This will likely only every change when developing/debugging.
		/// </summary>
		public const int SMTP_PORT = 25;

		private readonly ICfgPara _config;

		internal Client(ICfgPara config)
		{
			Guard.NotNull(config, nameof(config));

			_config = config;
		}

		/// <summary>
		/// The time in seconds after which an active but idle connection should be
		/// considered timed out.
		/// </summary>
		public int ConnectionIdleTimeoutInterval => _config.ClientIdleTimeout;

		/// <summary>
		/// The time in seconds for connection read timeouts.
		/// </summary>
		public int ConnectionReceiveTimeoutInterval => _config.ReceiveTimeout;

		/// <summary>
		/// The time in seconds for connection send timeouts.
		/// </summary>
		public int ConnectionSendTimeoutInterval => _config.SendTimeout;
	}

	/// <summary>
	/// Parameters regarding RabbitMQ.
	/// </summary>
	public class RabbitMQ
	{
		private bool _IsEnabled = false;
		private DateTimeOffset _IsEnabledLoadTime = DateTimeOffset.MinValue;
		private string _Username = string.Empty;
		private DateTimeOffset _UsernameLoadTime = DateTimeOffset.MinValue;
		private string _Password = string.Empty;
		private DateTimeOffset _PasswordLoadTime = DateTimeOffset.MinValue;
		private string _Hostname = string.Empty;
		private DateTimeOffset _HostnameLoadTime = DateTimeOffset.MinValue;
		private readonly ICfgPara _config;

		internal RabbitMQ(ICfgPara config)
		{
			Guard.NotNull(config, nameof(config));

			_config = config;
		}

		/// <summary>
		/// Will be true if MantaMTA should make use of RabbitMQ.
		/// </summary>
		public bool IsEnabled => _config.RabbitMqEnabled;

		/// <summary>
		/// Username for connecting to RabbitMQ.
		/// </summary>
		public string Username => _config.RabbitMqUsername;

		/// <summary>
		/// Password for connecting to RabbitMQ.
		/// </summary>
		public string Password => _config.RabbitMqPassword;

		/// <summary>
		/// Password for connecting to RabbitMQ.
		/// </summary>
		public string Hostname => _config.RabbitMqHostname;
	}

	/// <summary>
	/// Should be thrown when a Send is in a discarding state and an attempt is made to queue a message to it.
	/// </summary>
	public class SendDiscardingException : Exception { }

	/// <summary>
	/// Exception is thrown when an email is picked up for sending but there are no connections available and
	/// cannot attempt to create another as we've hit the maximum.
	/// </summary>
	public class MaxConnectionsException : Exception { }
}