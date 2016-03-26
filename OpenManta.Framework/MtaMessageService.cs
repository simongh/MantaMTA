using System;
using System.Threading.Tasks;
using OpenManta.Core;
using OpenManta.Data;

namespace OpenManta.Framework
{
	public static class MtaMessageHelper
	{
		/// <summary>
		/// This method handles message deferal.
		///	Logs deferral
		///	Fails the message if timed out
		/// or
		/// Sets the next rety date time
		/// </summary>
		/// <param name="defMsg">The deferal message from the SMTP server.</param>
		/// <param name="ipAddress">IP Address that send was attempted from.</param>
		/// <param name="mxRecord">MX Record of the server tried to send too.</param>
		/// <param name="isServiceUnavailable">If false will backoff the retry, if true will use the MtaParameters.MtaRetryInterval, 
		/// this is needed to reduce the tail when sending as a message could get multiple try again laters and soon be 1h+ before next retry.</param>
		public static void HandleDeliveryDeferral(MtaQueuedMessage msg, string defMsg, VirtualMTA ipAddress, MXRecord mxRecord, bool isServiceUnavailable = false)
		{
			HandleDeliveryDeferralAsync(msg, defMsg, ipAddress, mxRecord, isServiceUnavailable).Wait();
		}

		/// <summary>
		/// This method handles message deferal.
		///	Logs deferral
		///	Fails the message if timed out
		/// or
		/// Sets the next rety date time
		/// </summary>
		/// <param name="defMsg">The deferal message from the SMTP server.</param>
		/// <param name="ipAddress">IP Address that send was attempted from.</param>
		/// <param name="mxRecord">MX Record of the server tried to send too.</param>
		/// <param name="isServiceUnavailable">If false will backoff the retry, if true will use the MtaParameters.MtaRetryInterval, 
		/// this is needed to reduce the tail when sending as a message could get multiple try again laters and soon be 1h+ before next retry.</param>
		public static async Task<bool> HandleDeliveryDeferralAsync(MtaQueuedMessage msg, string defMsg, VirtualMTA ipAddress, MXRecord mxRecord, bool isServiceUnavailable = false, int? overrideTimeminutes = null)
		{
			// Log the deferral.
			await MtaTransaction.LogTransactionAsync(msg, TransactionStatus.Deferred, defMsg, ipAddress, mxRecord);

			// This holds the maximum interval between send retries. Should be put in the database.
			int maxInterval = 3 * 60;

			// Increase the defered count as the queued messages has been deferred.
			msg.DeferredCount++;

			// Hold the minutes to wait until next retry.
			double nextRetryInterval = MtaParameters.MtaRetryInterval;

			if (overrideTimeminutes.HasValue)
			{
				nextRetryInterval = overrideTimeminutes.Value;
			}
			else
			{
				if (!isServiceUnavailable)
				{
					// Increase the deferred wait interval by doubling for each retry.
					for (int i = 1; i < msg.DeferredCount; i++)
						nextRetryInterval = nextRetryInterval * 2;

					// If we have gone over the max interval then set to the max interval value.
					if (nextRetryInterval > maxInterval)
						nextRetryInterval = maxInterval;
				}
				else
					nextRetryInterval = 1; // For service unavalible use 1 minute between retries.
			}

			// Set next retry time and release the lock.
			msg.AttemptSendAfterUtc = DateTime.UtcNow.AddMinutes(nextRetryInterval);
			await Requeue(msg);

			return true;
		}

		/// <summary>
		/// This method handles failure of delivery.
		/// Logs failure
		/// Deletes queued data
		/// </summary>
		/// <param name="failMsg"></param>
		public static async Task<bool> HandleDeliveryFailAsync(MtaQueuedMessage msg, string failMsg, VirtualMTA ipAddress, MXRecord mxRecord)
		{
			await MtaTransaction.LogTransactionAsync(msg, TransactionStatus.Failed, failMsg, ipAddress, mxRecord);

			try
			{
				// Send fails to Manta.Core.Events
				for (int i = 0; i < msg.RcptTo.Length; i++)
				{
					EmailProcessingDetails processingInfo = null;
					EventsManager.Instance.ProcessSmtpResponseMessage(failMsg, msg.RcptTo[i], msg.InternalSendID, out processingInfo);
				}
			}
			catch (Exception)
			{

			}

			msg.IsHandled = true;

			return true;
		}

		/// <summary>
		/// This method handle successful delivery.
		/// Logs success
		/// Deletes queued data
		/// </summary>
		public static async Task<bool> HandleDeliverySuccessAsync(MtaQueuedMessage msg, VirtualMTA ipAddress, MXRecord mxRecord, string response)
		{
			await MtaTransaction.LogTransactionAsync(msg, TransactionStatus.Success, response, ipAddress, mxRecord);
			msg.IsHandled = true;
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ipAddress"></param>
		/// <param name="mxRecord"></param>
		/// <returns></returns>
		public static async Task<bool> HandleFailedToConnectAsync(MtaQueuedMessage msg, VirtualMTA ipAddress, MXRecord mxRecord)
		{
			// If there was no MX record in DNS, so using A, we should fail and not retry.
			if (mxRecord.MxRecordSrc == MxRecordSrc.A)
				return await HandleDeliveryFailAsync(msg, "550 Failed to connect", ipAddress, mxRecord);
			else
				return await HandleDeliveryDeferralAsync(msg, "Failed to connect", ipAddress, mxRecord, false, 15);
		}

		/// <summary>
		/// Discards the message.
		/// </summary>
		/// <param name="failMsg"></param>
		public static async Task<bool> HandleMessageDiscardAsync(MtaQueuedMessage msg)
		{
			await MtaTransaction.LogTransactionAsync(msg, TransactionStatus.Discarded, string.Empty, null, null);
			msg.IsHandled = true;
			return true;
		}
		/// <summary>
		/// This method handles message throttle.
		///	Logs throttle
		/// Sets the next rety date time 
		/// </summary>
		internal static async Task<bool> HandleDeliveryThrottleAsync(MtaQueuedMessage msg, VirtualMTA ipAddress, MXRecord mxRecord)
		{
			// Log deferral
			await MtaTransaction.LogTransactionAsync(msg, TransactionStatus.Throttled, string.Empty, ipAddress, mxRecord);

			// Set next retry time and release the lock.
			msg.AttemptSendAfterUtc = DateTime.UtcNow.AddMinutes(1);
			await Requeue(msg);
			return true;
		}

		/// <summary>
		/// Handle the message for a paused send.
		/// Should increase attempt send after timestamp and requeue in RabbitMQ.
		/// </summary>
		internal static async Task HandleSendPaused(MtaQueuedMessage msg)
		{
			msg.AttemptSendAfterUtc = DateTime.UtcNow.AddMinutes(1);
			await Requeue(msg);
		}

		/// <summary>
		/// Handles a service unavailable event, should be same as defer but only wait 1 minute before next retry.
		/// </summary>
		/// <param name="sndIpAddress"></param>
		internal static async Task<bool> HandleServiceUnavailableAsync(MtaQueuedMessage msg, VirtualMTA ipAddress)
		{ 
			// Log deferral
			await MtaTransaction.LogTransactionAsync(msg, TransactionStatus.Deferred, "Service Unavailable", ipAddress, null);

			// Set next retry time and release the lock.
			msg.AttemptSendAfterUtc = DateTime.UtcNow.AddSeconds(15);
			await Requeue(msg);
			return true;
		}
		/// <summary>
		/// Requeue the message in RabbitMQ.
		/// </summary>
		private static async Task Requeue(MtaQueuedMessage msg)
		{
			await RabbitMq.RabbitMqOutboundQueueManager.Enqueue(msg);
			msg.IsHandled = true;
		}
	}
}

