using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using OpenManta.Core;

namespace OpenManta.Data
{
	public static class OutboundRuleDBFactory
	{
		public static IOutboundRuleDB Instance { get; internal set; }
	}

	internal class OutboundRuleDB : IOutboundRuleDB
	{
		private readonly IDataRetrieval _dataRetrieval;
		private readonly IMantaDB _mantaDb;

		public OutboundRuleDB(IDataRetrieval dataRetrieval, IMantaDB mantaDb)
		{
			Guard.NotNull(dataRetrieval, nameof(dataRetrieval));

			_dataRetrieval = dataRetrieval;
		}

		/// <summary>
		/// Get the Outbound MX Patterns from the database.
		/// </summary>
		/// <returns></returns>
		public IList<OutboundMxPattern> GetOutboundRulePatterns()
		{
			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = @"
SELECT *
FROM man_rle_mxPattern
ORDER BY rle_mxPattern_id DESC"; // Order descending so default -1 is always at the bottom!

				return _dataRetrieval.GetCollectionFromDatabase<OutboundMxPattern>(cmd, CreateAndFillOutboundMxPattern);
			}
		}

		/// <summary>
		/// Get the Outbound Rules from the database.
		/// </summary>
		/// <returns></returns>
		public IList<OutboundRule> GetOutboundRules()
		{
			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = @"
SELECT *
FROM man_rle_rule";

				return _dataRetrieval.GetCollectionFromDatabase<OutboundRule>(cmd, CreateAndFillOutboundRule);
			}
		}

		/// <summary>
		/// Create and fill an OutboundMxPattern object from the Data Record.
		/// </summary>
		/// <param name="record">Datarecord containing values for the new object.</param>
		/// <returns>OutboundMxPattern object.</returns>
		private OutboundMxPattern CreateAndFillOutboundMxPattern(IDataRecord record)
		{
			OutboundMxPattern mxPattern = new OutboundMxPattern();

			mxPattern.Description = record.GetStringOrEmpty("rle_mxPattern_description");
			mxPattern.ID = record.GetInt32("rle_mxPattern_id");
			mxPattern.Name = record.GetString("rle_mxPattern_name");
			if (!record.IsDBNull("ip_ipAddress_id"))
				mxPattern.LimitedToOutboundIpAddressID = record.GetInt32("ip_ipAddress_id");
			mxPattern.Type = (OutboundMxPatternType)record.GetInt32("rle_patternType_id");
			mxPattern.Value = record.GetString("rle_mxPattern_value");
			return mxPattern;
		}

		/// <summary>
		/// Create and fill an OutboundRule object from the Data Record.
		/// </summary>
		/// <param name="record">Datarecord containing values for the new object.</param>
		/// <returns>OutboundRule object.</returns>
		private OutboundRule CreateAndFillOutboundRule(IDataRecord record)
		{
			OutboundRule rule = new OutboundRule(record.GetInt32("rle_mxPattern_id"), (OutboundRuleType)record.GetInt32("rle_ruleType_id"), record.GetString("rle_rule_value"));

			return rule;
		}
	}
}