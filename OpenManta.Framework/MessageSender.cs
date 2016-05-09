using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using OpenManta.Core;
using OpenManta.Framework.Smtp;
using OpenManta.Framework.RabbitMq;

namespace OpenManta.Framework
{
	/// <summary>
	/// MessageSender sends Emails to other servers from the Queue.
	/// </summary>
	public class MessageSender : IStopRequired
	{
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

		#region Singleton
		/// <summary>
		/// The Single instance of this class.
		/// </summary>
		private static MessageSender _Instance = new MessageSender();

        private MessageSender()
        {
            MantaCoreEvents.RegisterStopRequiredInstance(this);
        }

        /// <summary>
        /// Instance of the MessageSender class.
        /// </summary>
        public static MessageSender Instance
		{
			get
			{
				return MessageSender._Instance;
			}
		}
        #endregion

        /// <summary>
        /// If TRUE then request for client to stop has been made.
        /// </summary>
        private volatile bool _IsStopping = false;

        /// <summary>
        /// Holds the maximum amount of Tasks used for sending that should be run at anyone time.
        /// </summary>
        private int _MaxSendingWorkerTasks = -1;
		
		/// <summary>
		/// Holds the maximum amount of Tasks used for sending that should be run at anyone time.
		/// </summary>
		private int MAX_SENDING_WORKER_TASKS
		{
			get
			{
				if(_MaxSendingWorkerTasks == -1)
				{
					if (!int.TryParse(ConfigurationManager.AppSettings["MantaMaximumClientWorkers"], out _MaxSendingWorkerTasks))
					{
						Logging.Fatal("MantaMaximumClientWorkers not set in AppConfig");
						Environment.Exit(-1);
					}
					else if(_MaxSendingWorkerTasks < 1)
					{
						Logging.Fatal("MantaMaximumClientWorkers must be greater than 0");
						Environment.Exit(-1);
					}
					else
					{
						Logging.Info("Maximum Client Workers is " + _MaxSendingWorkerTasks.ToString());
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
                                // Send the message.
                                await SendMessageAsync(qMsg);

                                if (!qMsg.IsHandled)
                                {
                                    Logging.Warn("Message not handled " + qMsg.ID);
                                    qMsg.AttemptSendAfterUtc = DateTime.UtcNow.AddMinutes(6);
                                    await RabbitMqOutboundQueueManager.Enqueue(qMsg);
                                }

                                // Acknowledge of the message.
                                RabbitMqOutboundQueueManager.Ack(qMsg);

                                // Try to get another message to send.
                                qMsg = await RabbitMqOutboundQueueManager.Dequeue();
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log if we can't send the message.
                            Logging.Debug("Failed to send message", ex);
                        }
                        finally
                        {
                            // If there is still a acknowledge of the message.
                            if (qMsg != null)
                            {
                                if (!qMsg.IsHandled)
                                {
                                    Logging.Warn("Message not handled " + qMsg.ID);
                                    qMsg.AttemptSendAfterUtc = DateTime.UtcNow.AddMinutes(6);
                                    await RabbitMqOutboundQueueManager.Enqueue(qMsg);
                                }

								RabbitMqOutboundQueueManager.Ack(qMsg);
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
                        MtaQueuedMessage qmsg = RabbitMqOutboundQueueManager.Dequeue().Result;
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
			if (msg.AttemptSendAfterUtc > DateTime.UtcNow)
			{
				await RabbitMqOutboundQueueManager.Enqueue(msg);
				await Task.Delay(50); // To prevent a tight loop within a Task thread we should sleep here.
				return;
			}

			if (await Data.MtaTransaction.HasBeenHandledAsync(msg.ID))
			{
				msg.IsHandled = true;
				return;
			}

			// Get the send that this message belongs to so that we can check the send state.
			var snd = await SendManager.Instance.GetSendAsync(msg.InternalSendID);
			switch(snd.SendStatus)
			{
				// The send is being discarded so we should discard the message.
				case SendStatus.Discard:
				await MtaMessageHelper.HandleMessageDiscardAsync(msg);
					return;
				// The send is paused, the handle pause state will delay, without deferring, the message for a while so we can move on to other messages.
				case SendStatus.Paused:
				await MtaMessageHelper.HandleSendPaused(msg);
					return;
				// Send is active so we don't need to do anything.
				case SendStatus.Active:
					break;
				// Unknown send state, requeue the message and log error. Cannot send!
				default:
					msg.AttemptSendAfterUtc = DateTime.UtcNow.AddMinutes(1);
					await RabbitMqOutboundQueueManager.Enqueue(msg);
					Logging.Error("Failed to send message. Unknown SendStatus[" + snd.SendStatus + "]!");
					return;
			}
			
			// Check the message hasn't timed out. If it has don't attempt to send it.
			// Need to do this here as there may be a massive backlog on the server
			// causing messages to be waiting for ages after there AttemptSendAfter
			// before picking up. The MAX_TIME_IN_QUEUE should always be enforced.
			if (msg.AttemptSendAfterUtc - msg.QueuedTimestampUtc > new TimeSpan(0, MtaParameters.MtaMaxTimeInQueue, 0))
			{
				await MtaMessageHelper.HandleDeliveryFailAsync(msg, MtaParameters.TIMED_OUT_IN_QUEUE_MESSAGE, null, null);
			}
			else
			{
				MailAddress rcptTo = new MailAddress(msg.RcptTo[0]);
				MailAddress mailFrom = new MailAddress(msg.MailFrom);
				MXRecord[] mXRecords = DNSManager.GetMXRecords(rcptTo.Host);
				// If mxs is null then there are no MX records.
				if (mXRecords == null || mXRecords.Length < 1)
				{
					await MtaMessageHelper.HandleDeliveryFailAsync(msg, "550 Domain Not Found.", null, null);
				}
				else if(IsMxBlacklisted(mXRecords))
				{
					await MtaMessageHelper.HandleDeliveryFailAsync(msg, "550 Domain blacklisted.", null, mXRecords[0]);
				}
				else
				{
                    var vMtaGroup = VirtualMtaManager.GetVirtualMtaGroup(msg.VirtualMTAGroupID);
                    var sendResult = await MantaSmtpClientPoolCollection.Instance.SendAsync(mailFrom, rcptTo, vMtaGroup, mXRecords, msg.Message);
                    switch(sendResult.MantaOutboundClientResult)
                    {
                        case MantaOutboundClientResult.FailedToConnect:
                            await MtaMessageHelper.HandleFailedToConnectAsync(msg, sendResult.VirtualMTA, sendResult.MXRecord);
                            break;
                        case MantaOutboundClientResult.MaxConnections:
                        case MantaOutboundClientResult.MaxMessages:
                            await RabbitMqOutboundQueueManager.Enqueue(msg);
                            break;
                        case MantaOutboundClientResult.RejectedByRemoteServer:
                            if(string.IsNullOrWhiteSpace(sendResult.Message))
                            {
                                Logging.Error("RejectedByRemoteServer but no message!");
                                await MtaMessageHelper.HandleDeliveryDeferralAsync(msg, sendResult.Message, sendResult.VirtualMTA, sendResult.MXRecord);
                            }
                            if (sendResult.Message[0] == '4')
                                await MtaMessageHelper.HandleDeliveryDeferralAsync(msg, sendResult.Message, sendResult.VirtualMTA, sendResult.MXRecord);
                            else
                                await MtaMessageHelper.HandleDeliveryFailAsync(msg, sendResult.Message, sendResult.VirtualMTA, sendResult.MXRecord);
                            break;
                        case MantaOutboundClientResult.ServiceNotAvalible:
                            await MtaMessageHelper.HandleServiceUnavailableAsync(msg, sendResult.VirtualMTA);
                            break;
                        case MantaOutboundClientResult.Success:
                            await MtaMessageHelper.HandleDeliverySuccessAsync(msg, sendResult.VirtualMTA, sendResult.MXRecord, sendResult.Message);
                            break;
                        default:
                            // Something weird happening with this message, get it out of the way for a bit.
                            msg.AttemptSendAfterUtc = DateTime.UtcNow.AddMinutes(5);
                            await RabbitMqOutboundQueueManager.Enqueue(msg);
                            break;
                    }
				}
			}
		}
	}
}
