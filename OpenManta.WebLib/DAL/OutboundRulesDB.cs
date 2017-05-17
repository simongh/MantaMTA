using System;
using System.Data.SqlClient;
using OpenManta.Core;
using OpenManta.Data;
using OpenManta.Framework;

namespace OpenManta.WebLib.DAL
{
	internal class OutboundRulesDB : IOutboundRulesDB
	{
		private readonly IMantaDB _mantaDb;

		public OutboundRulesDB(IMantaDB mantaDb)
		{
			Guard.NotNull(mantaDb, nameof(mantaDb));

			_mantaDb = mantaDb;
		}

		/// <summary>
		/// Deletes the MX Pattern and its rules from the database.
		/// </summary>
		/// <param name="patternID">ID of the pattern to delete.</param>
		public void Delete(int mxPatternID)
		{
			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = @"
IF(@mxPatternID <> " + MtaParameters.OUTBOUND_RULES_DEFAULT_PATTERN_ID + @")
	BEGIN
		DELETE FROM Manta.Rules WHERE MxPatternId = @mxPatternID
		DELETE FROM Manta.MxPatterns WHERE MxPatternId = @mxPatternID
	END
";
				cmd.Parameters.AddWithValue("@mxPatternID", mxPatternID);
				conn.Open();
				cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Saves the OutboundRule to the database.
		/// </summary>
		/// <param name="outboundRule">The OutboundRule to save.</param>
		public void Save(OutboundRule outboundRule)
		{
			Guard.NotNull(outboundRule, nameof(outboundRule));

			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = @"
IF EXISTS (SELECT 1 FROM Manta.Rules WHERE MxPatternId = @mxPatternID AND RuleTypeId = @type)
	BEGIN
		UPDATE Manta.Rules
		SET Value = @value
		WHERE MxPatternId = @mxPatternID AND RuleTypeId = @type
	END
ELSE
	BEGIN
		INSERT INTO Manta.Rules(MxPatternId, RuleTypeId, value)
		VALUES(@mxPatternID, @type, @value)
	END
";
				cmd.Parameters.AddWithValue("@mxPatternID", outboundRule.OutboundMxPatternID);
				cmd.Parameters.AddWithValue("@type", (int)outboundRule.Type);
				cmd.Parameters.AddWithValue("@value", outboundRule.Value);
				conn.Open();
				cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Saves the specified OutboundMxPattern to the database.
		/// </summary>
		/// <param name="mxPattern">The OutboundMxPattern to save.</param>
		/// <returns>ID of the OutboundMxPattern.</returns>
		public int Save(OutboundMxPattern mxPattern)
		{
			Guard.NotNull(mxPattern, nameof(mxPattern));

			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = @"
IF EXISTS (SELECT 1 FROM Manta.MxPatterns WHERE MxPatternId = @mxPatternID)
	BEGIN
		UPDATE Manta.MxPatterns
		SET Name = @name,
		Description = @description,
		PatternTypeId = @type,
		Value = @value,
		IpAddressId = @ipAddressID
		WHERE MxPatternId = @mxPatternID

		SELECT @mxPatternID
	END
ELSE
	BEGIN
		INSERT INTO Manta.MxPatterns(Name, Description, PatternTypeId, Value, IpAddressId)
		VALUES(@name, @description, @type, @value, @ipAddressID)

		SELECT @@IDENTITY
	END
";
				cmd.Parameters.AddWithValue("@mxPatternID", mxPattern.ID);
				cmd.Parameters.AddWithValue("@name", mxPattern.Name);
				if (mxPattern.Description == null)
					cmd.Parameters.AddWithValue("@description", DBNull.Value);
				else
					cmd.Parameters.AddWithValue("@description", mxPattern.Description);
				cmd.Parameters.AddWithValue("@type", (int)mxPattern.Type);
				cmd.Parameters.AddWithValue("@value", mxPattern.Value);
				if (mxPattern.LimitedToOutboundIpAddressID.HasValue)
					cmd.Parameters.AddWithValue("@ipAddressID", mxPattern.LimitedToOutboundIpAddressID.Value);
				else
					cmd.Parameters.AddWithValue("@ipAddressID", DBNull.Value);
				conn.Open();
				return Convert.ToInt32(cmd.ExecuteScalar());
			}
		}
	}
}