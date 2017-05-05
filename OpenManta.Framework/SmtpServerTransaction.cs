using OpenManta.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenManta.Framework
{
	/// <summary>
	/// Represents the result of a call to an SmtpServerTransaction class async method.
	/// </summary>
	public enum SmtpServerTransactionAsyncResult
	{
		/// <summary>
		/// The method call resulted in an unknown state.
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// The message was successfully queued.
		/// </summary>
		SuccessMessageQueued = 1,

		/// <summary>
		/// The message was successfully delivered to a local mailbox.
		/// </summary>
		SuccessMessageDelivered = 2,

		/// <summary>
		/// The message was not queued as the Send it is apart of is discarding.
		/// </summary>
		FailedSendDiscarding = 3,

		FailedToEnqueue = 4
	}

	/// <summary>
	/// Represents an SMTP Server Transaction.
	/// That is a Transaction where we are the Server and someone is sending us stuff.
	/// </summary>
	internal class SmtpServerTransaction : ISmtpServerTransaction
	{
		private readonly IMessageManager _messageManager;
		private string _mailFrom;
		private bool _hasMailFrom;
		private readonly IVirtualMtaManager _virtualMtaManager;
		private readonly IQueueManager _queueManager;
		private readonly IMtaParameters _config;

		public SmtpServerTransaction(IMessageManager messageManager, IVirtualMtaManager virtualMtaManager, IQueueManager queueManager, IMtaParameters config)
		{
			Guard.NotNull(messageManager, nameof(messageManager));
			Guard.NotNull(virtualMtaManager, nameof(virtualMtaManager));
			Guard.NotNull(queueManager, nameof(queueManager));
			Guard.NotNull(config, nameof(config));

			_messageManager = messageManager;
			_virtualMtaManager = virtualMtaManager;
			_queueManager = queueManager;
			_config = config;

			RcptTo = new List<string>();
			MessageDestination = MessageDestination.Unknown;
			_hasMailFrom = false;
			Data = string.Empty;
			// Default value is set to 8bit as nearly all messages are sent using it.
			// Also some clients will send 8bit messages without passing a BODY parameter.
			TransportMIME = SmtpTransportMIME._8BitUTF;
		}

		/// <summary>
		/// The message data.
		/// </summary>
		public string Data { get; set; }

		/// <summary>
		/// FALSE until a MailFrom has been set.
		/// </summary>
		public bool HasMailFrom { get { return _hasMailFrom; } }

		/// <summary>
		/// The mail from.
		/// </summary>
		public string MailFrom
		{
			get
			{
				return _mailFrom;
			}
			set
			{
				_mailFrom = value;
				_hasMailFrom = true;
			}
		}

		/// <summary>
		/// The destination for this message.
		/// This should be set to inform us if the message should be put in the drop folder.
		/// Or saved to the database for relaying.
		/// </summary>
		public MessageDestination MessageDestination { get; set; }

		/// <summary>
		/// List of the recipients.
		/// </summary>
		public IList<string> RcptTo { get; set; }

		/// <summary>
		/// Holds the Transport MIME used to receive the Data message.
		/// </summary>
		public SmtpTransportMIME TransportMIME { get; set; }

		/// <summary>
		/// Adds a header to the message data.
		/// </summary>
		/// <param name="name">The header name.</param>
		/// <param name="value">Value for the header.</param>
		public void AddHeader(string name, string value)
		{
			Data = _messageManager.AddHeader(Data, new MessageHeader(name, value));
		}

		/// <summary>
		/// Save message(s) to DROP folder. Will place files in rcpt sub folder.
		/// OR
		/// Add message to queue for delivery (relay).
		/// </summary>
		public async Task<SmtpServerTransactionAsyncResult> SaveAsync()
		{
			SmtpServerTransactionAsyncResult result = SmtpServerTransactionAsyncResult.Unknown;

			if (MessageDestination == MessageDestination.Self)
				result = await SaveToLocalMailboxAsync();
			else if (MessageDestination == MessageDestination.Relay)
				result = await QueueForRelayingAsync();

			return result;
		}

		/// <summary>
		/// Queues the email for relaying.
		/// </summary>
		private async Task<SmtpServerTransactionAsyncResult> QueueForRelayingAsync()
		{
			// The email is for relaying.
			Guid messageID = Guid.NewGuid();

			// Look for any MTA control headers.
			MessageHeaderCollection headers = _messageManager.GetMessageHeaders(Data);

			// Will not be null if the SendGroupID header was present.
			MessageHeader ipGroupHeader = headers.SingleOrDefault(m => m.Name.Equals(MessageHeaderNames.SendGroupID, StringComparison.OrdinalIgnoreCase));

			// Parameter will hold the MtaIPGroup that will be used to relay this message.
			VirtualMtaGroup mtaGroup = null;
			int ipGroupID = 0;
			if (ipGroupHeader != null)
			{
				if (int.TryParse(ipGroupHeader.Value, out ipGroupID))
					mtaGroup = _virtualMtaManager.GetVirtualMtaGroup(ipGroupID);
			}

			#region Look for a send id, if one doesn't exist create it.

			MessageHeader sendIdHeader = headers.SingleOrDefault(h => h.Name.Equals(MessageHeaderNames.SendID, StringComparison.OrdinalIgnoreCase));
			int internalSendId = -1;
			if (sendIdHeader != null)
			{
				Send sndID = await SendManager.Instance.GetSendAsync(sendIdHeader.Value);
				if (sndID.SendStatus == SendStatus.Discard)
					return SmtpServerTransactionAsyncResult.FailedSendDiscarding;
				internalSendId = sndID.InternalID;
			}
			else
			{
				Send sndID = await SendManager.Instance.GetDefaultInternalSendIdAsync();
				if (sndID.SendStatus == SendStatus.Discard)
					return SmtpServerTransactionAsyncResult.FailedSendDiscarding;
				internalSendId = sndID.InternalID;
			}

			#endregion Look for a send id, if one doesn't exist create it.

			#region Generate Return Path

			string returnPath = string.Empty;

			// Can only return path to messages with one rcpt to
			if (RcptTo.Count == 1)
			{
				// Need to check to see if the message contains a return path overide domain.
				MessageHeader returnPathDomainOverrideHeader = headers.SingleOrDefault(h => h.Name.Equals(MessageHeaderNames.ReturnPathDomain, StringComparison.OrdinalIgnoreCase));

				if (returnPathDomainOverrideHeader != null &&
					_config.LocalDomains.Count(d => d.Hostname.Equals(returnPathDomainOverrideHeader.Value, StringComparison.OrdinalIgnoreCase)) > 0)
					// The message contained a local domain in the returnpathdomain
					// header so use it instead of the default.
					returnPath = ReturnPathManager.GenerateReturnPath(RcptTo[0], internalSendId, returnPathDomainOverrideHeader.Value);
				else
					// The message didn't specify a return path overide or it didn't
					// contain a localdomain so use the default.
					returnPath = ReturnPathManager.GenerateReturnPath(RcptTo[0], internalSendId);

				// Insert the return path header.
				Data = _messageManager.AddHeader(Data, new MessageHeader("Return-Path", "<" + returnPath + ">"));
			}
			else
			{
				// multiple rcpt's so can't have unique return paths, use generic mail from.
				returnPath = MailFrom;
			}

			#endregion Generate Return Path

			#region Generate a message ID header

			string msgIDHeaderVal = "<" + messageID.ToString("N") + MailFrom.Substring(MailFrom.LastIndexOf("@")) + ">";

			// If there is already a message header, remove it and add our own. required for feedback loop processing.
			if (headers.Count(h => h.Name.Equals("Message-ID", StringComparison.OrdinalIgnoreCase)) > 0)
				Data = _messageManager.RemoveHeader(Data, "Message-ID");

			// Add the new message-id header.
			Data = _messageManager.AddHeader(Data, new MessageHeader("Message-ID", msgIDHeaderVal));

			#endregion Generate a message ID header

			#region Get message priority

			var msgPriority = RabbitMqPriority.Low;
			var priorityHeader = headers.GetFirstOrDefault(MessageHeaderNames.Priority);
			if (priorityHeader != null)
			{
				var outVal = 0;
				if (int.TryParse(priorityHeader.Value, out outVal))
				{
					if (outVal >= 0)
						msgPriority = outVal < 3 ? (RabbitMqPriority)(byte)outVal
												 : RabbitMqPriority.High;
				}
			}

			#endregion Get message priority

			// Remove any control headers.
			headers = new MessageHeaderCollection(headers.Where(h => h.Name.StartsWith(MessageHeaderNames.HeaderNamePrefix, StringComparison.OrdinalIgnoreCase)));
			foreach (MessageHeader header in headers)
				Data = _messageManager.RemoveHeader(Data, header.Name);

			// If the MTA group doesn't exist or it's not got any IPs, use the default.
			if (mtaGroup == null ||
				mtaGroup.VirtualMtaCollection.Count == 0)
				ipGroupID = _virtualMtaManager.GetDefaultVirtualMtaGroup().ID;

			// Attempt to Enqueue the Email for Relaying.
			var enqueued = await _queueManager.Enqueue(messageID, ipGroupID, internalSendId, returnPath, RcptTo.ToArray(), Data, msgPriority);
			return enqueued ? SmtpServerTransactionAsyncResult.SuccessMessageQueued
							: SmtpServerTransactionAsyncResult.FailedToEnqueue;
		}

		/// <summary>
		/// Saves the email to the local drop folder.
		/// </summary>
		private async Task<SmtpServerTransactionAsyncResult> SaveToLocalMailboxAsync()
		{
			// Add the MAIL FROM & RCPT TO headers.
			Data = _messageManager.AddHeader(Data, new MessageHeader("X-Recipient", string.Join("; ", RcptTo)));
			if (HasMailFrom && string.IsNullOrWhiteSpace(MailFrom))
				Data = _messageManager.AddHeader(Data, new MessageHeader("X-Sender", "<>"));
			else
				Data = _messageManager.AddHeader(Data, new MessageHeader("X-Sender", MailFrom));

			// Need to drop a copy of the message for each recipient.
			for (int i = 0; i < RcptTo.Count; i++)
			{
				// Put the messages in a subfolder for each recipient.
				// Unless the rcpt to is a return path message in which case put them all in a return-path folder
				string mailDirPath = string.Empty;

				// Bounce.
				if (RcptTo[i].StartsWith("return-", StringComparison.OrdinalIgnoreCase))
					mailDirPath = _config.BounceDropFolder;

				// Abuse.
				else if (RcptTo[i].StartsWith("abuse@", StringComparison.OrdinalIgnoreCase))
					mailDirPath = _config.AbuseDropFolder;

				// Postmaster.
				else if (RcptTo[i].StartsWith("postmaster@", StringComparison.OrdinalIgnoreCase))
					mailDirPath = _config.PostmasterDropFolder;

				// Must be feedback loop.
				else
					mailDirPath = _config.FeedbackLoopDropFolder;

				// Ensure the directory exists by always calling create.
				Directory.CreateDirectory(mailDirPath);

				// Write the Email File.
				using (StreamWriter sw = new StreamWriter(Path.Combine(mailDirPath, Guid.NewGuid().ToString()) + ".eml"))
				{
					await sw.WriteAsync(Data);
				}
			}

			return SmtpServerTransactionAsyncResult.SuccessMessageDelivered;
		}
	}
}