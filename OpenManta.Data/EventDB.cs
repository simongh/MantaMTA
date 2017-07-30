using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using OpenManta.Core;

namespace OpenManta.Data
{
	//public static class EventDbFactory
	//{
	//	public static IEventDB Instance { get; internal set; }
	//}

	/// <summary>
	/// Performs database querying and retrieval operations for Manta's Events.
	/// </summary>
	internal class EventDB : IEventDB
	{
		private readonly IMantaDB _mantaDb;

		public EventDB(IMantaDB mantaDb)
		{
			Guard.NotNull(mantaDb, nameof(mantaDb));

			_mantaDb = mantaDb;
		}

		/// <summary>
		/// Retrieves all BounceRules from the database.
		/// </summary>
		/// <returns>A BounceRulesCollection of all the Rules.</returns>
		public BounceRulesCollection GetBounceRules()
		{
			var results = _mantaDb.GetCollectionFromDatabase<BounceRule>(@"
SELECT *
FROM Manta.BounceRules
ORDER BY ExecutionOrder ASC", CreateAndFillBounceRuleFromRecord);
			return new BounceRulesCollection(results);
		}

		/// <summary>
		/// Create and fill a BounceRule object from the Data Record.
		/// </summary>
		/// <param name="record">Datarecord containing values for the new object.</param>
		/// <returns>A BounceRule object.</returns>
		private BounceRule CreateAndFillBounceRuleFromRecord(IDataRecord record)
		{
			BounceRule rule = new BounceRule();

			rule.RuleID = record.GetInt32("BounceRuleId");
			rule.Name = record.GetString("Name");
			rule.Description = record.GetStringOrEmpty("Description");
			rule.ExecutionOrder = record.GetInt32("ExecutionOrder");
			rule.IsBuiltIn = record.GetBoolean("IsBuiltIn");
			rule.CriteriaType = (BounceRuleCriteriaType)record.GetInt32("BounceRuleCriteriaTypeId");
			rule.Criteria = record.GetString("Criteria");
			rule.BounceTypeIndicated = (MantaBounceType)record.GetInt32("BounceTypeId");
			rule.BounceCodeIndicated = (MantaBounceCode)record.GetInt32("BounceCodeId");

			return rule;
		}

		/// <summary>
		/// Gets a MantaEvent from the database.
		/// </summary>
		/// <returns>The event from the database of NULL if one wasn't found with the ID</returns>
		public MantaEvent GetEvent(int ID)
		{
			return _mantaDb.GetSingleObjectFromDatabase<MantaEvent>(@"
SELECT [evt].*, [bnc].BounceCodeId, [bnc].Message, [bnc].BounceTypeId
FROM Manta.Events AS [evt]
LEFT JOIN Manta.BounceEvents AS [bnc] ON [evt].EventId = [bnc].EventId
WHERE [evt].EventId = @eventId", CreateAndFillMantaEventFromRecord, cmd => cmd.Parameters.AddWithValue("@eventId", ID));
		}

		/// <summary>
		/// Gets all of the MantaEvents from the database.
		/// </summary>
		/// <returns>Collection of MantaEvent objects.</returns>
		public IList<MantaEvent> GetEvents()
		{
			return _mantaDb.GetCollectionFromDatabase<MantaEvent>(@"
SELECT [evt].*, [bnc].BounceCodeId, [bnc].Message, [bnc].BounceTypeId
FROM Manta.Events AS [evt]
LEFT JOIN Manta.BounceEvents AS [bnc] ON [evt].EventId = [bnc].EventId", CreateAndFillMantaEventFromRecord).ToList();
		}

		/// <summary>
		/// Gets <param name="maxEventsToGet"/> amount of Events that need forwarding from the database.
		/// </summary>
		public IList<MantaEvent> GetEventsForForwarding(int maxEventsToGet)
		{
			return _mantaDb.GetCollectionFromDatabase<MantaEvent>($@"
SELECT TOP {maxEventsToGet} [evt].*, [bnc].BounceCodeId, [bnc].Message, [bnc].BounceTypeId
FROM Manta.Events AS [evt]
LEFT JOIN Manta.BounceEvents AS [bnc] ON [evt].EventId = [bnc].EventId
WHERE IsForwarded = 0
ORDER BY EventId ASC", CreateAndFillMantaEventFromRecord).ToList();
		}

		/// <summary>
		/// Saves the MantaEvent to the database.
		/// </summary>
		/// <param name="evn">The Manta Event to save.</param>
		/// <returns>ID of the MantaEvent.</returns>
		public async Task<int> SaveAsync(MantaEvent evn)
		{
			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = @"
IF EXISTS (SELECT 1 FROM Manta.Events WHERE EventId = @eventID)
	BEGIN
		UPDATE Manta.Events
		SET EventTypeId = @eventType,
		CreatedAt = @timestamp,
		EmailAddress = @emailAddress,
		SendId = @sendId,
		IsForwarded = @forwarded
		WHERE EventId = @eventID
	END
ELSE
	BEGIN
		INSERT INTO Manta.Events(EventTypeId, CreatedAt, EmailAddress, SendId, IsForwarded)
		VALUES(@eventType, @timestamp, @emailAddress, @sendId, @forwarded)

		SET @eventID = @@IDENTITY
	END
";
				cmd.Parameters.AddWithValue("@eventID", evn.ID);
				cmd.Parameters.AddWithValue("@eventType", (int)evn.EventType);
				cmd.Parameters.AddWithValue("@timestamp", evn.EventTime);
				cmd.Parameters.AddWithValue("@emailAddress", evn.EmailAddress);
				cmd.Parameters.AddWithValue("@sendId", evn.SendID);
				cmd.Parameters.AddWithValue("@forwarded", evn.Forwarded);

				if (evn is MantaBounceEvent)
				{
					cmd.CommandText += @"
IF EXISTS (SELECT 1 FROM Manta.BounceEvents WHERE EventId = @eventId)
		UPDATE Manta.BounceEvents
		SET BounceCodeId = @bounceCode,
		Message = @message,
		BounceTypeId = @bounceType
		WHERE EventId = @eventId
ELSE
		INSERT INTO Manta.BounceEvents(EventId, BounceCodeId, Message, BounceTypeId)
		VALUES(@eventId, @bounceCode, @message, @bounceType)
";

					cmd.Parameters.AddWithValue("@bounceCode", (int)(evn as MantaBounceEvent).BounceInfo.BounceCode);
					cmd.Parameters.AddWithValue("@message", (evn as MantaBounceEvent).Message);
					cmd.Parameters.AddWithValue("@bounceType", (int)(evn as MantaBounceEvent).BounceInfo.BounceType);
				}

				cmd.CommandText += @"SELECT @eventID
";

				await conn.OpenAsync();
				return Convert.ToInt32(await cmd.ExecuteScalarAsync());
			}
		}

		/// <summary>
		/// Creates a MantaEvent object and Fills it with the values from the data record.
		/// </summary>
		/// <param name="record">Record to get the data from.</param>
		/// <returns>MantaAubseEvent or MantaBounceEvent</returns>
		private MantaEvent CreateAndFillMantaEventFromRecord(IDataRecord record)
		{
			MantaEventType type = (MantaEventType)record.GetInt32("EventTypeId");
			MantaEvent thisEvent = null;
			switch (type)
			{
				case MantaEventType.Abuse:
					thisEvent = new MantaAbuseEvent();
					break;

				case MantaEventType.Bounce:
					thisEvent = new MantaBounceEvent();
					FillMantaBounceEvent((thisEvent as MantaBounceEvent), record);
					break;

				case MantaEventType.TimedOutInQueue:
					thisEvent = new MantaTimedOutInQueueEvent();
					break;

				default:
					throw new NotImplementedException("Unknown Event Type (" + type + ")");
			}

			thisEvent.EmailAddress = record.GetString("EmailAddress");
			thisEvent.EventTime = record.GetDateTime("CreatedAt");
			thisEvent.EventType = type;
			thisEvent.ID = record.GetInt32("EventId");
			thisEvent.SendID = record.GetString("SendId");
			thisEvent.Forwarded = record.GetBoolean("IsForwarded");
			return thisEvent;
		}

		/// <summary>
		/// Fills the MantaBounceEvent with values from <paramref name="record"/>
		/// </summary>
		/// <param name="evt">The MantaBounceEvent to fill.</param>
		/// <param name="record">The data record to fill with.</param>
		private void FillMantaBounceEvent(MantaBounceEvent evt, IDataRecord record)
		{
			if (record.IsDBNull("BounceCodeId"))   // The bounce record is incomplete
			{
				evt.BounceInfo = new BouncePair
				{
					BounceCode = MantaBounceCode.Unknown,   // Don't know what the bounce was.
					BounceType = MantaBounceType.Soft // Assume soft bounce, just to be nice. If it happens 3 times sentoi will mark bad.
				};

				evt.Message = string.Empty; // There is no message.
			}
			else
			{
				evt.BounceInfo = new BouncePair
				{
					BounceCode = (MantaBounceCode)record.GetInt32("BounceCodeId"),
					BounceType = (MantaBounceType)record.GetInt32("BounceTypeId")
				};
				evt.Message = record.GetString("Message");
			}
		}
	}
}