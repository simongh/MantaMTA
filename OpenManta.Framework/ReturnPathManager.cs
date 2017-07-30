using System;
using System.Globalization;
using System.Threading.Tasks;
using OpenManta.Core;
using OpenManta.Data;

namespace OpenManta.Framework
{
	public class ReturnPathManager : IReturnPathManager
	{
		/// <summary>
		/// String that will be used to replace @ with in email address part of return path.
		/// </summary>
		private const string RCPT_TO_AT_REPLACEMENT = "=";

		private readonly IMtaParameters _config;
		private readonly IMtaMessageDB _messageDb;

		public ReturnPathManager(IMtaParameters config, IMtaMessageDB messageDb)
		{
			Guard.NotNull(config, nameof(config));
			Guard.NotNull(messageDb, nameof(messageDb));

			_config = config;
			_messageDb = messageDb;
		}

		/// <summary>
		/// Generates the return path, using the default return path domain.
		/// </summary>
		/// <param name="rcptTo">Email address where the message is going to.</param>
		/// <param name="internalSendID">The internal send id that the message relates to.</param>
		/// <returns>Return path.</returns>
		public string GenerateReturnPath(string rcptTo, int internalSendID)
		{
			return GenerateReturnPath(rcptTo, internalSendID, _config.ReturnPathDomain);
		}

		/// <summary>
		/// Generates the return path, using the specified return path domain.
		/// </summary>
		/// <param name="rcptTo">Email address where the message is going to.</param>
		/// <param name="internalSendID">The internal send id that the message relates to.</param>
		/// <param name="returnDomain">Domain that the return path should use.</param>
		/// <returns>Return path.</returns>
		public string GenerateReturnPath(string rcptTo, int internalSendID, string returnDomain)
		{
			Guard.NotNull(rcptTo, nameof(rcptTo));

			return string.Format("return-{0}-{1}@{2}",
						rcptTo.Replace("@", RCPT_TO_AT_REPLACEMENT),
						internalSendID.ToString("X"),
						returnDomain);
		}

		/// <summary>
		/// Try to decode the return path.
		/// </summary>
		/// <param name="returnPath">The return path to attempt to decode.</param>
		/// <param name="rcptTo">The email address the return path relates to.</param>
		/// <param name="internalSendID">The internal send id that the message was sent as part of.</param>
		/// <returns>True if successfully decoded; false if not.</returns>
		public bool TryDecode(string returnPath, out string rcptTo, out int internalSendID)
		{
			Guard.NotNull(returnPath, nameof(returnPath));

			try
			{
				int spos = returnPath.IndexOf("-") + 1;
				returnPath = returnPath.Substring(spos, returnPath.LastIndexOf("@") - spos);
				internalSendID = int.Parse(returnPath.Substring(returnPath.LastIndexOf("-") + 1), NumberStyles.HexNumber);
				rcptTo = returnPath.Substring(0, returnPath.LastIndexOf("-")).Replace(RCPT_TO_AT_REPLACEMENT, "@");
				return true;
			}
			catch (Exception)
			{
				rcptTo = null;
				internalSendID = -1;
				return false;
			}
		}

		/// <summary>
		/// Gets a return path from a MtaMessage ID.
		/// </summary>
		/// <param name="messageID">ID of the message.</param>
		/// <returns>The generated return path or string.empty if no message with ID found.</returns>
		public async Task<string> GetReturnPathFromMessageIDAsync(Guid messageID)
		{
			return await _messageDb.GetMailFrom(messageID).ConfigureAwait(false);
		}

		/// <summary>
		/// Gets a return path from a MtaMessage ID.
		/// </summary>
		/// <param name="messageID">ID of the message.</param>
		/// <returns>The generated return path or string.empty if no message with ID found.</returns>
		public string GetReturnPathFromMessageID(Guid messageID)
		{
			return GetReturnPathFromMessageIDAsync(messageID).Result;
		}
	}
}