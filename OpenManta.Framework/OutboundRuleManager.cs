using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using OpenManta.Core;
using OpenManta.Data;

namespace OpenManta.Framework
{
	internal class OutboundRuleManager : IOutboundRuleManager
	{
		/// <summary>
		/// Holds a cached copy of the Outbound MX Patterns from the database.
		/// </summary>
		private IList<OutboundMxPattern> _MXPatterns;

		/// <summary>
		/// Holds a cached copy of the Outbound Rules from the database.
		/// </summary>
		private IList<OutboundRule> _Rules;

		/// <summary>
		/// Holds a cached collection of matched patterns.
		/// Key: IP Address tostring()
		/// </summary>
		private ConcurrentDictionary<string, MatchedMxPatternCollection> _matchedPatterns;

		/// <summary>
		/// Class represents a matched MX Pattern
		/// </summary>
		private class MatchedMxPattern
		{
			/// <summary>
			/// ID of the pattern that resulted in this match.
			/// </summary>
			public int MxPatternID { get; set; }

			/// <summary>
			/// IP Address if specific otherwise string.empty.
			/// </summary>
			public string IPAddress { get; set; }

			/// <summary>
			/// DateTime of the match.
			/// </summary>
			public DateTime MatchedUtc { get; set; }

			public MatchedMxPattern()
			{
				this.MxPatternID = -1;
				this.IPAddress = null;
			}
		}

		/// <summary>
		/// Holds a collection of matched MX patterns
		/// Key: MX Record hostname.
		/// </summary>
		private class MatchedMxPatternCollection : ConcurrentDictionary<string, MatchedMxPattern>
		{
			/// <summary>
			/// Adds or updates.
			/// </summary>
			/// <param name="mxPatternID">The matching pattern ID</param>
			/// <param name="ipAddress">IP Address if specific or NULL</param>
			public void Add(int mxPatternID, VirtualMTA ipAddress)
			{
				OutboundRuleManager.MatchedMxPattern newMxPattern = new OutboundRuleManager.MatchedMxPattern();
				newMxPattern.MatchedUtc = DateTime.UtcNow;
				newMxPattern.MxPatternID = mxPatternID;

				Func<string, OutboundRuleManager.MatchedMxPattern, OutboundRuleManager.MatchedMxPattern> updateAction = delegate (string key, OutboundRuleManager.MatchedMxPattern existing)
				{
					if (existing.MatchedUtc > newMxPattern.MatchedUtc)
						return existing;
					return newMxPattern;
				};

				if (ipAddress != null)
				{
					newMxPattern.IPAddress = ipAddress.IPAddress.ToString();
					this.AddOrUpdate(newMxPattern.IPAddress,
									 new MatchedMxPattern()
									 {
										 MatchedUtc = DateTime.UtcNow,
										 MxPatternID = mxPatternID
									 }, updateAction);
				}
				else
					this.AddOrUpdate(string.Empty,
									 new OutboundRuleManager.MatchedMxPattern
									 {
										 MatchedUtc = DateTime.UtcNow,
										 MxPatternID = mxPatternID
									 }, updateAction);
			}

			/// <summary>
			/// Gets the matched MX Record. Null if not found.
			/// </summary>
			/// <param name="ipAddress"></param>
			/// <returns></returns>
			public MatchedMxPattern GetMatchedMxPattern(VirtualMTA ipAddress)
			{
				OutboundRuleManager.MatchedMxPattern tmp;
				if (base.TryGetValue(ipAddress.IPAddress.ToString(), out tmp))
				{
					if (tmp.MatchedUtc.AddMinutes(MtaParameters.MTA_CACHE_MINUTES) < DateTime.UtcNow)
						return tmp;
				}
				else
				{
					if (base.TryGetValue(string.Empty, out tmp))
					{
						if (tmp.MatchedUtc.AddMinutes(MtaParameters.MTA_CACHE_MINUTES) > DateTime.UtcNow)
							return tmp;
					}
				}

				return null;
			}
		}

		private readonly IOutboundRuleDB _ruleDb;
		private readonly ILog _logging;
		private readonly IMantaCoreEvents _coreEvents;

		public OutboundRuleManager(IOutboundRuleDB ruleDb, ILog logging, IMantaCoreEvents coreEvents)
		{
			Guard.NotNull(ruleDb, nameof(ruleDb));
			Guard.NotNull(logging, nameof(logging));
			Guard.NotNull(coreEvents, nameof(coreEvents));

			_ruleDb = ruleDb;
			_logging = logging;
			_coreEvents = coreEvents;
		}

		/// <summary>
		/// Gets the Outbound Rules for the specified destination MX and optionally IP Address.
		/// </summary>
		/// <param name="mxRecord">MXRecord for the destination MX.</param>
		/// <param name="mtaIpAddress">Outbound IP Address</param>
		/// <param name="mxPatternID">OUT: the ID of MxPattern that caused match.</param>
		/// <returns></returns>
		public IList<OutboundRule> GetRules(MXRecord mxRecord, VirtualMTA mtaIpAddress, out int mxPatternID)
		{
			Guard.NotNull(mxRecord, nameof(mxRecord));
			Guard.NotNull(mtaIpAddress, nameof(mtaIpAddress));

			// Get the data from the database. This needs to be cleverer and reload every x minutes.
			if (_MXPatterns == null)
				_MXPatterns = _ruleDb.GetOutboundRulePatterns();
			if (_Rules == null)
				_Rules = _ruleDb.GetOutboundRules();

			int patternID = GetMxPatternID(mxRecord, mtaIpAddress);
			mxPatternID = patternID;
			return (from r in _Rules
					where r.OutboundMxPatternID == patternID
					select r).ToList();
		}

		/// <summary>
		/// Gets the MxPatternID that matches the MX Record, Outbound IP Address combo.
		/// </summary>
		/// <param name="record"></param>
		/// <param name="ipAddress"></param>
		/// <returns></returns>
		private int GetMxPatternID(MXRecord record, VirtualMTA ipAddress)
		{
			if (_matchedPatterns == null)
				_matchedPatterns = new ConcurrentDictionary<string, MatchedMxPatternCollection>();

			MatchedMxPatternCollection matchedPatterns = null;

			if (!_matchedPatterns.TryGetValue(record.Host, out matchedPatterns))
			{
				matchedPatterns = new MatchedMxPatternCollection();
				_matchedPatterns.AddOrUpdate(record.Host, matchedPatterns, (string s, MatchedMxPatternCollection p) => matchedPatterns);
			}

			MatchedMxPattern matchedPattern = matchedPatterns.GetMatchedMxPattern(ipAddress);

			if (matchedPattern != null &&
				matchedPattern.MatchedUtc.AddMinutes(MtaParameters.MTA_CACHE_MINUTES) > DateTime.UtcNow)
				// Found a valid cached pattern ID so return it.
				return matchedPattern.MxPatternID;

			// Loop through all of the patterns
			for (int i = 0; i < _MXPatterns.Count; i++)
			{
				// The current pattern we're working with.
				OutboundMxPattern pattern = _MXPatterns[i];

				// If the pattern applies only to a specified IP address then
				// only check for a match if getting rules for that IP.
				if (pattern.LimitedToOutboundIpAddressID.HasValue &&
					pattern.LimitedToOutboundIpAddressID.Value != ipAddress.ID)
					continue;

				if (pattern.Type == OutboundMxPatternType.CommaDelimited)
				{
					// Pattern is a comma delimited list, so split the values
					string[] strings = pattern.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

					// Loop though the values in the split string array.
					for (int c = 0; c < strings.Length; c++)
					{
						try
						{
							// If they are a match return the rules.
							if (strings[c].Equals(record.Host, StringComparison.OrdinalIgnoreCase))
							{
								if (pattern.LimitedToOutboundIpAddressID.HasValue)
									matchedPatterns.Add(pattern.ID, ipAddress);
								else
									matchedPatterns.Add(pattern.ID, null);

								return pattern.ID;
							}
						}
						catch (Exception) { }
					}

					continue;
				}
				else if (pattern.Type == OutboundMxPatternType.Regex)
				{
					// Pattern is Regex so just need to do an IsMatch
					if (Regex.IsMatch(record.Host, pattern.Value, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture))
					{
						// Found pattern match.
						if (pattern.LimitedToOutboundIpAddressID.HasValue)
							matchedPatterns.Add(pattern.ID, ipAddress);
						else
							matchedPatterns.Add(pattern.ID, null);

						return pattern.ID;
					}
					else
						continue;
				}
				else
				{
					// Don't know what to do with this pattern so move on to the next.
					_logging.Error("Unknown OutboundMxPatternType : " + pattern.Type.ToString());
					continue;
				}
			}

			// Should have been found by default at least, but hasn't.
			_logging.Fatal("No MX Pattern Rules! Default Deleted?");
			_coreEvents.InvokeMantaCoreStopping();
			Environment.Exit(0);
			return -1;
		}

		/// <summary>
		/// Gets the MAX number of messages allowed to be sent through the connection.
		/// </summary>
		/// <param name="record">MX Record for the destination.</param>
		/// <param name="ipAddress">IPAddress that we are sending from.</param>
		/// <returns>Max number of messages per connection.</returns>
		public int GetMaxMessagesPerConnection(MXRecord record, VirtualMTA ipAddress)
		{
			Guard.NotNull(record, nameof(record));
			Guard.NotNull(ipAddress, nameof(ipAddress));

			int mxPatternID = 0;
			IList<OutboundRule> rules = GetRules(record, ipAddress, out mxPatternID);
			for (int i = 0; i < rules.Count; i++)
			{
				if (rules[i].Type == OutboundRuleType.MaxMessagesConnection)
				{
					int tmp = 0;
					if (int.TryParse(rules[i].Value, out tmp))
						return tmp;
					else
					{
						_logging.Error("Failed to get max messages per connection for " + record.Host + " using " + ipAddress.IPAddress.ToString() + " value wasn't valid [" + rules[i].Value + "], defaulting to 1");
						return 1;
					}
				}
			}

			_logging.Error("Failed to get max messages per connection for " + record.Host + " using " + ipAddress.IPAddress.ToString() + " defaulting to 1");
			return 1;
		}

		public int GetMaxMessagesDestinationHour(VirtualMTA vmta, MXRecord mx)
		{
			Guard.NotNull(vmta, nameof(vmta));
			Guard.NotNull(mx, nameof(mx));

			int tmp;
			return GetMaxMessagesDestinationHour(vmta, mx, out tmp);
		}

		/// <summary>
		/// Gets the maximum amount of messages to send per hour from each ip address to mx.
		/// </summary>
		/// <param name="ipAddress">Outbound IP address</param>
		/// <param name="record">MX Record of destination server.</param>
		/// <param name="mxPatternID">ID of the pattern used to identify the rule.</param>
		/// <returns>Maximum number of messages per hour or -1 for unlimited.</returns>
		public int GetMaxMessagesDestinationHour(VirtualMTA ipAddress, MXRecord record, out int mxPatternID)
		{
			Guard.NotNull(ipAddress, nameof(ipAddress));
			Guard.NotNull(record, nameof(record));

			IList<OutboundRule> rules = GetRules(record, ipAddress, out mxPatternID);
			for (int i = 0; i < rules.Count; i++)
			{
				if (rules[i].Type == OutboundRuleType.MaxMessagesPerHour)
				{
					int tmp = 0;
					if (int.TryParse(rules[i].Value, out tmp))
						return tmp;
					else
					{
						_logging.Error("Failed to get max messages per hour for " + record.Host + " using " + ipAddress.IPAddress.ToString() + " value wasn't valid [" + rules[i].Value + "], defaulting to unlimited");
						return -1;
					}
				}
			}

			_logging.Error("Failed to get max messages per hour for " + record.Host + " using " + ipAddress.IPAddress.ToString() + " defaulting to unlimited");
			return -1;
		}

		/// <summary>
		/// Gets the maximum amount of simultaneous connections to specified host.
		/// </summary>
		/// <param name="ipAddress">IP Address connecting from.</param>
		/// <param name="record">MXRecord of the destination.</param>
		/// <returns>Max number of connections.</returns>
		public int GetMaxConnectionsToDestination(VirtualMTA ipAddress, MXRecord record)
		{
			Guard.NotNull(ipAddress, nameof(ipAddress));
			Guard.NotNull(record, nameof(record));

			int mxPatternID = -1;
			IList<OutboundRule> rules = GetRules(record, ipAddress, out mxPatternID);
			for (int i = 0; i < rules.Count; i++)
			{
				if (rules[i].Type == OutboundRuleType.MaxConnections)
				{
					int tmp = 0;
					if (int.TryParse(rules[i].Value, out tmp))
						return tmp;
					else
					{
						_logging.Error("Failed to get max connections for " + record.Host + " using " + ipAddress.IPAddress.ToString() + " value wasn't valid [" + rules[i].Value + "], defaulting to 1");
						return 1;
					}
				}
			}

			_logging.Error("Failed to get max connections for " + record.Host + " using " + ipAddress.IPAddress.ToString() + " defaulting to 1");
			return 1;
		}
	}
}