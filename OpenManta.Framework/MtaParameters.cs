using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;
using OpenManta.Core;
using OpenManta.Data;

namespace OpenManta.Framework
{
	public class MtaParameters : IMtaParameters
	{
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

		private static DateTimeOffset _MtaDropFolderLoadTime = DateTimeOffset.MinValue;
		private static DateTimeOffset _MtaQueueFolderLoadTime = DateTimeOffset.MinValue;
		private static DateTimeOffset _MtaLogFolderLoadTime = DateTimeOffset.MinValue;
		private static DateTimeOffset _LocalDomainsLoadTime = DateTimeOffset.MinValue;
		private static DateTimeOffset _ReturnPathDomainLoadTime = DateTimeOffset.MinValue;
		private static DateTimeOffset _IPsToAllowRelayingLoadTime = DateTimeOffset.MinValue;
		private static DateTimeOffset _MtaRetryIntervalLoadTime = DateTimeOffset.MinValue;
		private static int _MtaMaxTimeInQueue = -1;
		private static DateTimeOffset _MtaMaxTimeInQueueLoadTime = DateTimeOffset.MinValue;
		private static bool _keepBounceFiles = false;
		private static DateTimeOffset _keepBounceFilesLoadTime = DateTimeOffset.MinValue;
		private static int _DaysToKeepSmtpLogsFor = -1;
		private static string _EventForwardingHttpPostUrl = string.Empty;
		private static int _MtaRetryInterval = -1;
		private static IList<LocalDomain> _LocalDomains;
		private static string _MtaDropFolder;
		private static int[] _ServerListeningPorts;
		private static string _MtaQueueFolder;
		private static string _MtaLogFolder;
		private static string _ReturnPathDomain = string.Empty;
		private static string[] _IPsToAllowRelaying;
		private readonly ICfgPara _config;
		private readonly ICfgLocalDomains _localDomainConfig;
		private readonly ICfgRelayingPermittedIP _permittedIpConfig;
		private readonly ILog _logging;

		public MtaParameters(ICfgPara config, ICfgLocalDomains localDomainConfig, ICfgRelayingPermittedIP permittedIpConfig, log4net.ILog logging)
		{
			Guard.NotNull(config, nameof(config));
			Guard.NotNull(localDomainConfig, nameof(localDomainConfig));
			Guard.NotNull(permittedIpConfig, nameof(permittedIpConfig));
			Guard.NotNull(logging, nameof(logging));

			_config = config;
			_localDomainConfig = localDomainConfig;
			_permittedIpConfig = permittedIpConfig;
			_logging = logging;

			Client = new Client(config);
			RabbitMq = new RabbitMQ(config);
		}

		/// <summary>
		/// Gets the ports that the SMTP server should listen for client connections on.
		/// This will almost always be 25 & 587.
		/// </summary>
		public IEnumerable<int> ServerListeningPorts
		{
			get
			{
				if (_ServerListeningPorts == null)
					_ServerListeningPorts = _config.ServerListenPorts.ToArray();
				return _ServerListeningPorts;
			}
		}

		/// <summary>
		/// Drop folder, for incoming messages.
		/// This should be in config.
		/// </summary>
		public string MTA_DROPFOLDER
		{
			get
			{
				if (_MtaDropFolderLoadTime < DateTimeOffset.UtcNow)
				{
					_MtaDropFolder = _config.DropFolder;
					Directory.CreateDirectory(_MtaDropFolder);
					_MtaDropFolderLoadTime = DateTimeOffset.UtcNow.AddMinutes(MTA_CACHE_MINUTES);
				}

				return _MtaDropFolder;
			}
		}

		/// <summary>
		/// Drop folder for abuse@
		/// </summary>
		public string AbuseDropFolder
		{
			get
			{
				return Path.Combine(MTA_DROPFOLDER, "abuse");
			}
		}

		/// <summary>
		/// Drop folder for email bounces.
		/// </summary>
		public string BounceDropFolder
		{
			get
			{
				return Path.Combine(MTA_DROPFOLDER, "bounce");
			}
		}

		/// <summary>
		/// Drop folder for feedback loop emails.
		/// </summary>
		public string FeedbackLoopDropFolder
		{
			get
			{
				return Path.Combine(MTA_DROPFOLDER, "feedback");
			}
		}

		/// <summary>
		/// Drop folder for postmaster@
		/// </summary>
		public string PostmasterDropFolder
		{
			get
			{
				return Path.Combine(MTA_DROPFOLDER, "postmaster");
			}
		}

		/// <summary>
		/// Queue folder, for messages to be sent.
		/// </summary>
		public string MTA_QUEUEFOLDER
		{
			get
			{
				if (_MtaQueueFolderLoadTime < DateTimeOffset.UtcNow)
				{
					_MtaQueueFolder = _config.QueueFolder;
					Directory.CreateDirectory(_MtaQueueFolder);
					_MtaQueueFolderLoadTime = DateTimeOffset.UtcNow.AddMinutes(MTA_CACHE_MINUTES);
				}

				return _MtaQueueFolder;
			}
		}

		/// <summary>
		/// Log foler, where SMTP Transaction logs will go.
		/// This should be in config.
		/// </summary>
		public string MTA_SMTP_LOGFOLDER
		{
			get
			{
				if (_MtaLogFolderLoadTime < DateTimeOffset.UtcNow)
				{
					_MtaLogFolder = _config.LogFolder;
					Directory.CreateDirectory(_MtaLogFolder);
					_MtaLogFolderLoadTime = DateTimeOffset.UtcNow.AddMinutes(MTA_CACHE_MINUTES);
				}

				return _MtaLogFolder;
			}
		}

		/// <summary>
		/// List of domains to accept messages for drop folder.
		/// All domains are toLowered!
		/// </summary>
		public IList<LocalDomain> LocalDomains
		{
			get
			{
				if (_LocalDomainsLoadTime < DateTimeOffset.UtcNow)
				{
					_LocalDomains = _localDomainConfig.GetLocalDomainsArray();
					_LocalDomainsLoadTime = DateTimeOffset.UtcNow.AddMinutes(5);
				}
				return _LocalDomains;
			}
		}

		/// <summary>
		/// The domain that return paths should use.
		/// </summary>
		public string ReturnPathDomain
		{
			get
			{
				if (_ReturnPathDomainLoadTime < DateTimeOffset.UtcNow)
				{
					_ReturnPathDomain = _config.ReturnPathDomainId.ToString();
					_ReturnPathDomainLoadTime = DateTimeOffset.UtcNow.AddMinutes(MTA_CACHE_MINUTES);
				}
				return _ReturnPathDomain;
			}
		}

		/// <summary>
		/// List of IP addresses to allow relaying for.
		/// </summary>
		public string[] IPsToAllowRelaying
		{
			get
			{
				if (_IPsToAllowRelayingLoadTime < DateTimeOffset.UtcNow)
				{
					_IPsToAllowRelaying = _permittedIpConfig.GetRelayingPermittedIPAddresses().ToArray();
					_IPsToAllowRelayingLoadTime = DateTimeOffset.UtcNow.AddMinutes(MTA_CACHE_MINUTES);
				}
				return _IPsToAllowRelaying;
			}
		}

		/// <summary>
		/// The time in minutes between send retries.
		/// </summary>
		public int MtaRetryInterval
		{
			get
			{
				if (_MtaRetryIntervalLoadTime < DateTimeOffset.UtcNow)
				{
					_MtaRetryInterval = _config.RetryIntervalBaseMinutes;
					_MtaRetryIntervalLoadTime = DateTimeOffset.UtcNow.AddMinutes(5);
				}

				return _MtaRetryInterval;
			}
		}

		/// <summary>
		/// The maximum time in minutes that a message can be in the queue.
		/// </summary>
		public int MtaMaxTimeInQueue
		{
			get
			{
				if (_MtaMaxTimeInQueueLoadTime < DateTimeOffset.UtcNow)
				{
					_MtaMaxTimeInQueue = _config.MaxTimeInQueueMinutes;
					_MtaMaxTimeInQueueLoadTime = DateTimeOffset.UtcNow.AddMinutes(5);
				}

				return _MtaMaxTimeInQueue;
			}
		}

		/// <summary>
		/// Flag to indicate whether to retain succesfully processed bounce email files.  Used to see how bounces have been processed so
		/// the processing code can be reviewed and Bounce Rules modified if necessary.
		///
		/// Files that result in an error when being processed are always kept.
		///
		/// If true, successfully processed bounce email files are kept in folders relating to how they were identified;
		/// if false, they are immediately deleted.
		/// </summary>
		public bool KeepBounceFiles
		{
			get
			{
				if (_keepBounceFilesLoadTime < DateTimeOffset.UtcNow)
				{
					bool newFlag = _config.KeepBounceFilesFlag;

					if (newFlag != _keepBounceFiles)
					{
						// Log that there was a change so we're aware that bounce files are being kept.
						_logging.Info("Bounce Files are " + (newFlag ? "now" : "no longer") + " being kept.");
					}

					_keepBounceFiles = newFlag;
					_keepBounceFilesLoadTime = DateTimeOffset.UtcNow.AddMinutes(MTA_CACHE_MINUTES);
				}

				return _keepBounceFiles;
			}
		}

		/// <summary>
		/// The amount of days to keep SMTP logs for before deleting them.
		/// </summary>
		public int DaysToKeepSmtpLogsFor
		{
			get
			{
				if (_DaysToKeepSmtpLogsFor == -1)
					_DaysToKeepSmtpLogsFor = _config.DaysToKeepSmtpLogsFor;
				return _DaysToKeepSmtpLogsFor;
			}
		}

		/// <summary>
		/// The URL to post Manta Events (abuse/bounce) to.
		/// </summary>
		public Uri EventForwardingHttpPostUrl
		{
			get
			{
				if (string.IsNullOrEmpty(_EventForwardingHttpPostUrl))
					_EventForwardingHttpPostUrl = _config.EventForwardingHttpPostUrl;
				if (string.IsNullOrEmpty(_EventForwardingHttpPostUrl))
					return null;

				return new Uri(_EventForwardingHttpPostUrl);
			}
		}

		public Client Client { get; private set; }

		public RabbitMQ RabbitMq { get; private set; }
	}

	public class Client
	{
		/// <summary>
		/// Port for SMTP connections by the client to remote servers when sending
		/// messages. This will likely only every change when developing/debugging.
		/// </summary>
		public const int SMTP_PORT = 25;

		private int _ConnectionIdleTimeoutInterval = -1;
		private DateTimeOffset _ConnectionIdleTimeoutIntervalLoadTime = DateTimeOffset.MinValue;
		private int _ConnectionReceiveTimeoutInterval = -1;
		private DateTimeOffset _ConnectionReceiveTimeoutIntervalLoadTime = DateTimeOffset.MinValue;
		private int _connectionSendTimeoutInterval = -1;
		private DateTimeOffset _connectionSendTimeoutIntervalLoadTime = DateTimeOffset.MinValue;
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
		public int ConnectionIdleTimeoutInterval
		{
			get
			{
				if (_ConnectionIdleTimeoutIntervalLoadTime < DateTimeOffset.UtcNow)
				{
					_ConnectionIdleTimeoutInterval = _config.ClientIdleTimeout;
					_ConnectionIdleTimeoutIntervalLoadTime = DateTimeOffset.UtcNow.AddMinutes(MtaParameters.MTA_CACHE_MINUTES);
				}

				return _ConnectionIdleTimeoutInterval;
			}
		}

		/// <summary>
		/// The time in seconds for connection read timeouts.
		/// </summary>
		public int ConnectionReceiveTimeoutInterval
		{
			get
			{
				if (_ConnectionReceiveTimeoutIntervalLoadTime < DateTimeOffset.UtcNow)
				{
					_ConnectionReceiveTimeoutInterval = _config.ReceiveTimeout;
					_ConnectionReceiveTimeoutIntervalLoadTime = DateTimeOffset.UtcNow.AddMinutes(MtaParameters.MTA_CACHE_MINUTES);
				}

				return _ConnectionReceiveTimeoutInterval;
			}
		}

		/// <summary>
		/// The time in seconds for connection send timeouts.
		/// </summary>
		public int ConnectionSendTimeoutInterval
		{
			get
			{
				if (_connectionSendTimeoutIntervalLoadTime < DateTimeOffset.UtcNow)
				{
					_connectionSendTimeoutInterval = _config.SendTimeout;
					_connectionSendTimeoutIntervalLoadTime = DateTimeOffset.UtcNow.AddMinutes(MtaParameters.MTA_CACHE_MINUTES);
				}

				return _connectionSendTimeoutInterval;
			}
		}
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
		public bool IsEnabled
		{
			get
			{
				if (_IsEnabledLoadTime < DateTimeOffset.UtcNow)
				{
					_IsEnabled = _config.RabbitMqEnabled;
					_IsEnabledLoadTime = DateTimeOffset.UtcNow.AddMinutes(MtaParameters.MTA_CACHE_MINUTES);
				}

				return _IsEnabled;
			}
		}

		/// <summary>
		/// Username for connecting to RabbitMQ.
		/// </summary>
		public string Username
		{
			get
			{
				if (_UsernameLoadTime < DateTimeOffset.UtcNow)
				{
					_Username = _config.RabbitMqUsername;
					_UsernameLoadTime = DateTimeOffset.UtcNow.AddMinutes(MtaParameters.MTA_CACHE_MINUTES);
				}

				return _Username;
			}
		}

		/// <summary>
		/// Password for connecting to RabbitMQ.
		/// </summary>
		public string Password
		{
			get
			{
				if (_PasswordLoadTime < DateTimeOffset.UtcNow)
				{
					_Password = _config.RabbitMqPassword;
					_UsernameLoadTime = DateTimeOffset.UtcNow.AddMinutes(MtaParameters.MTA_CACHE_MINUTES);
				}

				return _Password;
			}
		}

		/// <summary>
		/// Password for connecting to RabbitMQ.
		/// </summary>
		public string Hostname
		{
			get
			{
				if (_HostnameLoadTime < DateTimeOffset.UtcNow)
				{
					_Hostname = _config.RabbitMqHostname;
					_HostnameLoadTime = DateTimeOffset.UtcNow.AddMinutes(MtaParameters.MTA_CACHE_MINUTES);
				}

				return _Hostname;
			}
		}
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