using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using OpenManta.Core;
using OpenManta.Framework.Queues;
using OpenManta.Framework.Smtp;

namespace OpenManta.Framework
{
	/// <summary>
	/// MessageSender sends Emails to other servers from the Queue.
	/// </summary>
	internal class MessageSender : IMessageSender, IStopRequired
	{
		/// <summary>
		/// If TRUE then request for client to stop has been made.
		/// </summary>
		private volatile bool _IsStopping = false;

		/// <summary>
		/// Holds the maximum amount of Tasks used for sending that should be run at anyone time.
		/// </summary>
		private int _MaxSendingWorkerTasks = -1;

		/// <summary>
		/// List of MX domains that we should not attempt to deliver to. The emails will hard bounce as "Domain Blacklisted".
		/// Todo: Put this in database and web interface.
		/// </summary>
		private List<string> _blacklistMx = new List<string> {
			".",
			"anmail.namebrightmail.com",
			"amail.germanmails.biz",
			"mx-uk.newses.de",
			"spamgoes.in",
			"uk-com-wildcard-null-mx.centralnic.net",
			"yudifuta.weirdcups.com", // Spamlist
            "erick555.servehttp.com",
			"freeletter.me",
			"muirfieldcontracts.co.uk",
			"techgroup.me",
			"exchange.uk.com",
			"inboxdesign.me",
			"banff-buchan.ac.uk"
		};

		private readonly Data.IMtaTransaction _mtaTransaction;
		private readonly ILog _logging;
		private readonly IDnsManager _dnsManager;
		private readonly IMtaMessageHelper _messageHelper;
		private readonly IVirtualMtaManager _virtualMtaManager;
		private readonly IMantaSmtpClientPoolCollection _clientPools;
		private readonly IMtaParameters _config;
		private readonly IOutboundQueueManager _queueManager;

		public MessageSender(Data.IMtaTransaction mtaTransaction, IMantaCoreEvents coreEvents, ILog logging, IDnsManager dnsManager, IMtaMessageHelper messageHelper, IVirtualMtaManager virtualMtaManager, IMantaSmtpClientPoolCollection clientPools, IMtaParameters config, IOutboundQueueManager queueManager)
		{
			Guard.NotNull(mtaTransaction, nameof(mtaTransaction));
			Guard.NotNull(coreEvents, nameof(coreEvents));
			Guard.NotNull(logging, nameof(logging));
			Guard.NotNull(dnsManager, nameof(dnsManager));
			Guard.NotNull(messageHelper, nameof(messageHelper));
			Guard.NotNull(virtualMtaManager, nameof(virtualMtaManager));
			Guard.NotNull(clientPools, nameof(clientPools));
			Guard.NotNull(config, nameof(config));
			Guard.NotNull(queueManager, nameof(queueManager));

			_mtaTransaction = mtaTransaction;
			_logging = logging;
			_dnsManager = dnsManager;
			_messageHelper = messageHelper;
			_virtualMtaManager = virtualMtaManager;
			_clientPools = clientPools;
			_config = config;
			_queueManager = queueManager;

			coreEvents.RegisterStopRequiredInstance(this);
		}

		/// <summary>
		/// Holds the maximum amount of Tasks used for sending that should be run at anyone time.
		/// </summary>
		private int MAX_SENDING_WORKER_TASKS
		{
			get
			{
				if (_MaxSendingWorkerTasks == -1)
				{
					if (!int.TryParse(ConfigurationManager.AppSettings["MantaMaximumClientWorkers"], out _MaxSendingWorkerTasks))
					{
						_logging.Fatal("MantaMaximumClientWorkers not set in AppConfig");
						Environment.Exit(-1);
					}
					else if (_MaxSendingWorkerTasks < 1)
					{
						_logging.Fatal("MantaMaximumClientWorkers must be greater than 0");
						Environment.Exit(-1);
					}
					else
					{
						_logging.Info("Maximum Client Workers is " + _MaxSendingWorkerTasks.ToString());
					}
				}

				return _MaxSendingWorkerTasks;
			}
		}

		public void Start()
		{
			Thread t = new Thread(new ThreadStart(() =>
			{
				// Dictionary will hold a single int for each running task. The int means nothing.
				ConcurrentDictionary<Guid, int> runningTasks = new ConcurrentDictionary<Guid, int>();

				// Send the message.
				Action<MtaQueuedMessage> taskWorker = (qMsg) =>
				{
					// Generate a unique ID for this task.
					Guid taskID = Guid.NewGuid();

					// Add this task to the running list.
					if (!runningTasks.TryAdd(taskID, 1))
						return;

					Task.Factory.StartNew(async () =>
					{
						try
						{
							// Loop while there is a task message to send.
							while (qMsg != null && !_IsStopping)
							{
								await SendMessageAsync(qMsg);

								if (!qMsg.IsHandled)
								{
									_logging.Warn("Message not handled " + qMsg.ID);
									qMsg.AttemptSendAfterUtc = DateTimeOffset.UtcNow.AddMinutes(5);
									await _queueManager.Enqueue(qMsg);
								}

								// Acknowledge of the message.
								_queueManager.Ack(qMsg);

								// Try to get another message to send.
								qMsg = await _queueManager.Dequeue();
							}
						}
						catch (Exception ex)
						{
							// Log if we can't send the message.
							_logging.Debug("Failed to send message", ex);
						}
						finally
						{
							// If there is still a acknowledge of the message.
							if (qMsg != null)
							{
								if (!qMsg.IsHandled)
								{
									_logging.Warn("Message not handled " + qMsg.ID);
									qMsg.AttemptSendAfterUtc = DateTimeOffset.UtcNow.AddMinutes(5);
									await _queueManager.Enqueue(qMsg);
								}

								_queueManager.Ack(qMsg);
							}

							// Remove this task from the dictionary
							int value;
							runningTasks.TryRemove(taskID, out value);
						}
					}, TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness);
				};

				Action startWorkerTasks = () =>
				{
					while ((runningTasks.Count < MAX_SENDING_WORKER_TASKS) && !_IsStopping)
					{
						MtaQueuedMessage qmsg = _queueManager.Dequeue().Result;
						if (qmsg == null)
							break; // Nothing to do, so don't start anymore workers.

						taskWorker(qmsg);
					}
				};

				do
				{
					if (runningTasks.Count >= MAX_SENDING_WORKER_TASKS)
					{
						Thread.Sleep(100);
						continue;
					}

					startWorkerTasks();
				} while (!_IsStopping);
			}));
			t.Start();
		}

		/// <summary>
		/// IStopRequired method. Will be called by MantaCoreEvents on stopping of MTA.
		/// </summary>
		public void Stop()
		{
			_IsStopping = true;
		}

		/// <summary>
		/// Checks to see if the MX record collection contains blacklisted domains/ips.
		/// </summary>
		/// <param name="mxRecords">Collection of MX records to check.</param>
		/// <returns>True if collection contains blacklisted record.</returns>
		private bool IsMxBlacklisted(MXRecord[] mxRecords)
		{
			// Check for blacklisted MX
			foreach (var mx in mxRecords)
			{
				if (_blacklistMx.Contains(mx.Host.ToLower()))
					return true;
			}

			return false;
		}

		private async Task SendMessageAsync(MtaQueuedMessage msg)
		{
			// Check that the message next attempt after has passed.
			if (msg.AttemptSendAfterUtc > DateTimeOffset.UtcNow)
			{
				await _queueManager.Enqueue(msg);
				await Task.Delay(50); // To prevent a tight loop within a Task thread we should sleep here.
				return;
			}

			if (await _mtaTransaction.HasBeenHandledAsync(msg.ID))
			{
				msg.IsHandled = true;
				return;
			}

			// Get the send that this message belongs to so that we can check the send state.
			var snd = await SendManager.Instance.GetSendAsync(msg.InternalSendID);
			switch (snd.SendStatus)
			{
				// The send is being discarded so we should discard the message.
				case SendStatus.Discard:
					await _messageHelper.HandleMessageDiscardAsync(msg);
					return;
				// The send is paused, the handle pause state will delay, without deferring, the message for a while so we can move on to other messages.
				case SendStatus.Paused:
					await _messageHelper.HandleSendPaused(msg);
					return;
				// Send is active so we don't need to do anything.
				case SendStatus.Active:
					break;
				// Unknown send state, requeue the message and log error. Cannot send!
				default:
					msg.AttemptSendAfterUtc = DateTimeOffset.UtcNow.AddMinutes(1);
					await _queueManager.Enqueue(msg);
					_logging.Error("Failed to send message. Unknown SendStatus[" + snd.SendStatus + "]!");
					return;
			}

			// Check the message hasn't timed out. If it has don't attempt to send it.
			// Need to do this here as there may be a massive backlog on the server
			// causing messages to be waiting for ages after there AttemptSendAfter
			// before picking up. The MAX_TIME_IN_QUEUE should always be enforced.
			if (msg.AttemptSendAfterUtc - msg.QueuedTimestampUtc > new TimeSpan(0, _config.MtaMaxTimeInQueue, 0))
			{
				await _messageHelper.HandleDeliveryFailAsync(msg, MtaParameters.TIMED_OUT_IN_QUEUE_MESSAGE, null, null);
			}
			else
			{
				MailAddress rcptTo = new MailAddress(msg.RcptTo[0]);
				MailAddress mailFrom = new MailAddress(msg.MailFrom);
				MXRecord[] mXRecords = _dnsManager.GetMXRecords(rcptTo.Host).ToArray();
				// If mxs is null then there are no MX records.
				if (mXRecords == null || mXRecords.Length < 1)
				{
					await _messageHelper.HandleDeliveryFailAsync(msg, "550 Domain Not Found.", null, null);
				}
				else if (IsMxBlacklisted(mXRecords))
				{
					await _messageHelper.HandleDeliveryFailAsync(msg, "550 Domain blacklisted.", null, mXRecords[0]);
				}
				else
				{
					var vMtaGroup = _virtualMtaManager.GetVirtualMtaGroup(msg.VirtualMTAGroupID);
					var sendResult = await _clientPools.SendAsync(mailFrom, rcptTo, vMtaGroup, mXRecords, msg.Message);
					switch (sendResult.MantaOutboundClientResult)
					{
						case MantaOutboundClientResult.FailedToConnect:
							await _messageHelper.HandleFailedToConnectAsync(msg, sendResult.VirtualMTA, sendResult.MXRecord);
							break;

						case MantaOutboundClientResult.MaxConnections:
							await _queueManager.Enqueue(msg);
							break;

						case MantaOutboundClientResult.MaxMessages:
							await _messageHelper.HandleDeliveryThrottleAsync(msg, sendResult.VirtualMTA, sendResult.MXRecord);
							break;

						case MantaOutboundClientResult.RejectedByRemoteServer:
							if (string.IsNullOrWhiteSpace(sendResult.Message))
							{
								_logging.Error("RejectedByRemoteServer but no message!");
								await _messageHelper.HandleDeliveryDeferralAsync(msg, sendResult.Message, sendResult.VirtualMTA, sendResult.MXRecord);
							}
							if (sendResult.Message[0] == '4')
								await _messageHelper.HandleDeliveryDeferralAsync(msg, sendResult.Message, sendResult.VirtualMTA, sendResult.MXRecord);
							else
								await _messageHelper.HandleDeliveryFailAsync(msg, sendResult.Message, sendResult.VirtualMTA, sendResult.MXRecord);
							break;

						case MantaOutboundClientResult.ServiceNotAvalible:
							await _messageHelper.HandleServiceUnavailableAsync(msg, sendResult.VirtualMTA);
							break;

						case MantaOutboundClientResult.Success:
							await _messageHelper.HandleDeliverySuccessAsync(msg, sendResult.VirtualMTA, sendResult.MXRecord, sendResult.Message);
							break;

						default:
							// Something weird happening with this message, get it out of the way for a bit.
							msg.AttemptSendAfterUtc = DateTimeOffset.UtcNow.AddMinutes(5);
							await _queueManager.Enqueue(msg);
							break;
					}
				}
			}
		}
	}
}