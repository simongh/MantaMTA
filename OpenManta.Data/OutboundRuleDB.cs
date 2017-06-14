using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using OpenManta.Core;

namespace OpenManta.Data
{
	public static class OutboundRuleDBFactory
	{
		public static IOutboundRuleDB Instance { get; internal set; }
	}

	internal class OutboundRuleDB : IOutboundRuleDB
	{
		private readonly IMantaDB _mantaDb;

		public OutboundRuleDB(IMantaDB mantaDb)
		{
			Guard.NotNull(mantaDb, nameof(mantaDb));

			_mantaDb = mantaDb;
		}

		/// <summary>
		/// Get the Outbound MX Patterns from the database.
		/// </summary>
		/// <returns></returns>
		public IList<OutboundMxPattern> GetOutboundRulePatterns()
		{
			return _mantaDb.GetCollectionFromDatabase<OutboundMxPattern>(@"
SELECT *
FROM Manta.MxPatterns
ORDER BY MxPatternId DESC", CreateAndFillOutboundMxPattern).ToList();
		}

		/// <summary>
		/// Get the Outbound Rules from the database.
		/// </summary>
		/// <returns></returns>
		public IList<OutboundRule> GetOutboundRules()
		{
			return _mantaDb.GetCollectionFromDatabase<OutboundRule>(@"
SELECT *
FROM Manta.Rules", CreateAndFillOutboundRule).ToList();
		}

		/// <summary>
		/// Create and fill an OutboundMxPattern object from the Data Record.
		/// </summary>
		/// <param name="record">Datarecord containing values for the new object.</param>
		/// <returns>OutboundMxPattern object.</returns>
		private OutboundMxPattern CreateAndFillOutboundMxPattern(IDataRecord record)
		{
			OutboundMxPattern mxPattern = new OutboundMxPattern();

			mxPattern.Description = record.GetStringOrEmpty("Description");
			mxPattern.ID = record.GetInt32("MxPatternId");
			mxPattern.Name = record.GetString("Name");
			if (!record.IsDBNull("IpAddressId"))
				mxPattern.LimitedToOutboundIpAddressID = record.GetInt32("IpAddressId");
			mxPattern.Type = (OutboundMxPatternType)record.GetInt32("PatternTypeId");
			mxPattern.Value = record.GetString("Value");
			return mxPattern;
		}

		/// <summary>
		/// Create and fill an OutboundRule object from the Data Record.
		/// </summary>
		/// <param name="record">Datarecord containing values for the new object.</param>
		/// <returns>OutboundRule object.</returns>
		private OutboundRule CreateAndFillOutboundRule(IDataRecord record)
		{
			OutboundRule rule = new OutboundRule(record.GetInt32("MxPatternId"), (OutboundRuleType)record.GetInt32("RuleTypeId"), record.GetString("Value"));

			return rule;
		}
	}
}