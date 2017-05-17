using FluentMigrator;
using FluentMigrator.Runner.Extensions;

namespace OpenManta.Data.Migrations
{
	[Migration(1)]
	public class Initial : Migration
	{
		private const string SchemaName = "manta";

		public override void Down()
		{
			//Delete.Index("IX_man_mta_sendMeta").OnTable("man_mta_sendMeta");

			//Delete.Index("IX_man_mta_transaction").OnTable("man_mta_transaction");

			//Delete.Index("BounceGroupingIndex").OnTable("man_mta_transaction");

			//Delete.Index("mta_msg_id").OnTable("man_mta_msg");

			//Delete.Index("AttemptSendAfter").OnTable("man_mta_queue");

			//Delete.Index("NonClusteredIndex-20141117-122722").OnTable("man_mta_queue");

			Delete.Table("LocalDomains").InSchema(SchemaName);//man_cfg_localDomain

			Delete.Table("Settings").InSchema(SchemaName);//man_cfg_para

			Delete.Table("PermittedRelayIps").InSchema(SchemaName);//man_cfg_relayingPermittedIp

			Delete.Table("BounceRules").InSchema(SchemaName);//man_evn_bounceRule

			Delete.Table("BounceEvents").InSchema(SchemaName);//man_evn_bounceEvent

			Delete.Table("BounceCodes").InSchema(SchemaName);//man_evn_bounceCode

			Delete.Table("BounceRuleCriteriaTypes").InSchema(SchemaName);//man_evn_bounceRuleCriteriaType

			Delete.Table("BounceTypes").InSchema(SchemaName);//man_evn_bounceType

			Delete.Table("Events").InSchema(SchemaName);//man_evn_event

			Delete.Table("EventTypes").InSchema(SchemaName);//man_evn_type

			Delete.Table("FeedbackLoopAddresses").InSchema(SchemaName);//man_mta_fblAddress

			Delete.Table("Queue").InSchema(SchemaName);//man_mta_queue

			Delete.Table("Transactions").InSchema(SchemaName);//man_mta_transaction

			Delete.Table("Messages").InSchema(SchemaName);//man_mta_msg

			Delete.Table("SendMetadata").InSchema(SchemaName);//man_mta_sendMeta

			Delete.Table("MtaSend").InSchema(SchemaName);//man_mta_send

			Delete.Table("SendStatuses").InSchema(SchemaName);//man_mta_sendStatus

			Delete.Table("IpGroupMembers").InSchema(SchemaName);//man_ip_groupMembership

			Delete.Table("IpGroups").InSchema(SchemaName);//man_ip_group

			Delete.Table("IpAddresses").InSchema(SchemaName);//man_ip_ipAddress

			Delete.Table("TransactionStatuses").InSchema(SchemaName);//man_mta_transactionStatus

			Delete.Table("Rules").InSchema(SchemaName);//man_rle_rule

			Delete.Table("RuleTypes").InSchema(SchemaName);//man_rle_ruleType

			Delete.Table("MxPatterns").InSchema(SchemaName);//man_rle_mxPattern

			Delete.Table("PatternTypes").InSchema(SchemaName);//man_rle_patternType
		}

		public override void Up()
		{
			if (!Schema.Schema(SchemaName).Exists())
				Create.Schema(SchemaName);

			Create.Table("LocalDomains")//man_cfg_localDomain
				.InSchema(SchemaName)
				.WithIdColumn("LocalDomainId")//cfg_localDomain_id
				.WithColumn("Domain").AsString(255).NotNullable()//cfg_localDomain_domain
				.WithColumn("Name").AsString(50).Nullable()//cfg_localDomain_name
				.WithDescriptionColumn();//cfg_localDomain_description

			Create.Table("Settings")//man_cfg_para
				.InSchema(SchemaName)
				.WithIdColumn("SettingId")
				.WithColumn("Name").AsString(100).NotNullable()
				.WithColumn("Value").AsString().Nullable();
			//.WithColumn("").AsString(255).NotNullable()//cfg_para_dropFolder
			//.WithColumn("").AsString(255).NotNullable()//cfg_para_queueFolder
			//.WithColumn("").AsString(255).NotNullable()//cfg_para_logFolder
			//.WithColumn("").AsString(255).NotNullable()//cfg_para_listenPorts
			//.WithColumn("").AsInt32().NotNullable()//cfg_para_retryIntervalMinutes
			//.WithColumn("").AsInt32().NotNullable()//cfg_para_maxTimeInQueueMinutes
			//.WithColumn("").AsInt32().NotNullable()//cfg_para_defaultIpGroupId
			//.WithColumn("").AsInt32().NotNullable()//cfg_para_clientIdleTimeout
			//.WithColumn("").AsInt32().NotNullable()//cfg_para_receiveTimeout
			//.WithColumn("").AsInt32().NotNullable()//cfg_para_sendTimeout
			//.WithColumn("").AsInt32().NotNullable()//cfg_para_returnPathDomain_id
			//.WithColumn("").AsInt32().NotNullable()//cfg_para_maxDaysToKeepSmtpLogs
			//.WithColumn("").AsString().NotNullable()//cfg_para_eventForwardingHttpPostUrl
			//.WithColumn("").AsBoolean().NotNullable()//cfg_para_keepBounceFilesFlag
			//.WithColumn("").AsBoolean().NotNullable()//cfg_para_rabbitMqEnabled
			//.WithColumn("").AsString().NotNullable()//cfg_para_rabbitMqUsername
			//.WithColumn("").AsString().NotNullable()//cfg_para_rabbitMqPassword
			//.WithColumn("").AsString().NotNullable();//cfg_para_rabbitMqHostname

			Create.Table("PermittedRelayIps")//man_cfg_relayingPermittedIp
				.InSchema(SchemaName)
				.WithIdColumn("PermittedRelayIpId")
				.WithColumn("IpAddress").AsString(45).NotNullable().Unique()//cfg_relayingPermittedIp_ip
				.WithColumn("Name").AsString(50).Nullable()//cfg_relayingPermittedIp_name
				.WithDescriptionColumn();//cfg_relayingPermittedIp_description

			Create.Table("BounceCodes")//man_evn_bounceCode
				.InSchema(SchemaName)
				.WithLookupId("BounceCodeId")//evn_bounceCode_id
				.WithNameColumn()//evn_bounceCode_name
				.WithDescriptionColumn();//evn_bounceCode_description

			Create.Table("BounceRuleCriteriaTypes")//man_evn_bounceRuleCriteriaType
				.InSchema(SchemaName)
				.WithLookupId("BounceRuleCriteriaTypeId")//evn_bounceRuleCriteriaType_id
				.WithNameColumn()//evn_bounceRuleCriteriaType_name
				.WithDescriptionColumn();//evn_bounceRuleCriteriaType_description

			Create.Table("BounceTypes")//man_evn_bounceType
				.InSchema(SchemaName)
				.WithLookupId("BounceTypeId")//evn_bounceType_id
				.WithNameColumn()//evn_bounceType_name
				.WithDescriptionColumn();//evn_bounceType_description

			Create.Table("BounceRules")//man_evn_bounceRule
				.InSchema(SchemaName)
				.WithLookupId("BounceRuleId")//evn_bounceRule_id
				.WithNameColumn()//evn_bounceRule_name
				.WithDescriptionColumn()//evn_bounceRule_description
				.WithColumn("ExecutionOrder").AsInt32().NotNullable()//evn_bounceRule_executionOrder
				.WithColumn("IsBuiltIn").AsBoolean().NotNullable()//evn_bounceRule_isBuiltIn
				.WithColumn("BounceRuleCriteriaTypeId").AsInt32().NotNullable()//evn_bounceRuleCriteriaType_id
				.WithColumn("Criteria").AsString().NotNullable()//evn_bounceRule_criteria
				.WithColumn("BounceTypeId").AsInt32().NotNullable()//evn_bounceRule_mantaBounceType
				.WithColumn("BounceCodeId").AsInt32().NotNullable();//evn_bounceRule_mantaBounceCode

			Create.QuickForeignKey(SchemaName, "BounceRules", "BounceRuleCriteriaTypes", "BounceRuleCriteriaTypeId");

			Create.QuickForeignKey(SchemaName, "BounceRules", "BounceTypes", "BounceTypeId");

			Create.QuickForeignKey(SchemaName, "BounceRules", "BounceCodes", "BounceCodeId");

			Create.Table("BounceEvents")//man_evn_bounceEvent
				.InSchema(SchemaName)
				.WithColumn("EventId").AsInt32().PrimaryKey()//evn_event_id
				.WithColumn("BounceCodeId").AsInt32().NotNullable()//evn_bounceCode_id
				.WithColumn("BounceTypeId").AsInt32().NotNullable()//evn_bounceType_id
				.WithColumn("Message").AsString().NotNullable();//evn_bounceEvent_message

			Create.QuickForeignKey(SchemaName, "BounceEvents", "Events", "EventId");

			Create.QuickForeignKey(SchemaName, "BounceEvents", "BounceTypes", "BounceTypeId");

			Create.QuickForeignKey(SchemaName, "BounceEvents", "BounceCodes", "BounceCodeId");

			Create.Table("EventTypes")//man_evn_type
				.InSchema(SchemaName)
				.WithLookupId("EventTypeId")//evn_type_id
				.WithNameColumn()//evn_type_name
				.WithDescriptionColumn();//evn_type_description

			Create.Table("Events")//man_evn_event
				.InSchema(SchemaName)
				.WithIdColumn("EventId")//evn_event_id
				.WithColumn("EventTypeId").AsInt32().NotNullable()//evn_type_id
				.WithColumn("CreatedAt").AsDateTimeOffset().NotNullable()//evn_event_timestamp
				.WithColumn("EmailAddress").AsString(320).NotNullable()//evn_event_emailAddress
				.WithColumn("SendId").AsString(20).NotNullable()//snd_send_id
				.WithColumn("IsForwarded").AsBoolean().NotNullable().WithDefaultValue(0);//evn_event_forwarded

			Create.QuickForeignKey(SchemaName, "Events", "EventTypes", "EventTypeId");

			Create.Table("IpGroups")//man_ip_group
				.InSchema(SchemaName)
				.WithIdColumn("IpGroupId")//ip_group_id
				.WithNameColumn()//ip_group_name
				.WithDescriptionColumn();//ip_group_description

			Create.Table("IpAddresses")//man_ip_ipAddress
				.InSchema(SchemaName)
				.WithIdColumn("IpAddressId")//ip_ipAddress_id
				.WithColumn("IpAddress").AsAnsiString(45).NotNullable()//ip_ipAddress_ipAddress
				.WithColumn("Hostname").AsAnsiString(255).Nullable()//ip_ipAddress_hostname
				.WithColumn("IsInbound").AsBoolean().Nullable()//ip_ipAddress_isInbound
				.WithColumn("IsOutbound").AsBoolean().Nullable();//ip_ipAddress_isOutbound

			Create.Table("IpGroupMembers")//man_ip_groupMembership
				.InSchema(SchemaName)
				.WithColumn("IpGroupId").AsInt32().NotNullable().PrimaryKey()//ip_group_id
				.WithColumn("IpAddressId").AsInt32().NotNullable().PrimaryKey();//ip_ipAddress_id

			Create.QuickForeignKey(SchemaName, "IpGroupMembers", "IpAddresses", "IpAddressId");

			Create.QuickForeignKey(SchemaName, "IpGroupMembers", "IpGroups", "IpGroupId");

			Create.Table("FeedbackLoopAddresses")//man_mta_fblAddress
				.InSchema(SchemaName)
				.WithIdColumn("FeedbackLoopAddressId")
				.WithColumn("Address").AsString(320).NotNullable()//mta_fblAddress_address
				.WithNameColumn()//mta_fblAddress_name
				.WithDescriptionColumn();//mta_fblAddress_description

			Create.Table("SendStatuses")//man_mta_sendStatus
				.InSchema(SchemaName)
				.WithLookupId("SendStatusId")//mta_sendStatus_id
				.WithNameColumn()//mta_sendStatus_name
				.WithDescriptionColumn();//mta_sendStatus_description

			Create.Table("MtaSend")//man_mta_send
				.InSchema(SchemaName)
				.WithColumn("MtaSendId").AsInt32().NotNullable().PrimaryKey()//mta_send_internalId
				.WithColumn("SendId").AsString(20).NotNullable()//mta_send_id
				.WithColumn("SendStatusId").AsInt32().NotNullable()//mta_sendStatus_id
				.WithColumn("CreatedAt").AsDateTimeOffset().NotNullable()//mta_send_createdTimestamp
				.WithColumn("Messages").AsInt32().NotNullable().WithDefaultValue(0)//mta_send_messages
				.WithColumn("Accepted").AsInt32().NotNullable().WithDefaultValue(0)//mta_send_accepted
				.WithColumn("Rejected").AsInt32().NotNullable().WithDefaultValue(0);//mta_send_rejected

			Create.QuickForeignKey(SchemaName, "MtaSend", "SendStatuses", "SendStatusId");

			Create.Table("Queue")//man_mta_queue
				.InSchema(SchemaName)
				.WithColumn("MessageId").AsGuid().NotNullable().PrimaryKey()//mta_msg_id
				.WithColumn("CreatedAt").AsDateTimeOffset().NotNullable()//mta_queue_queuedTimestamp
				.WithColumn("AttemptSendAfter").AsDateTimeOffset().NotNullable()//mta_queue_attemptSendAfter
				.WithColumn("IsPickupLocked").AsBoolean().NotNullable()//mta_queue_isPickupLocked
				.WithColumn("DataPath").AsString().NotNullable()//mta_queue_dataPath
				.WithColumn("GroupId").AsInt32().NotNullable()//ip_group_id
				.WithColumn("MtaSendId").AsInt32().NotNullable();//mta_send_internalId

			Create.Table("Messages")//man_mta_msg
				.InSchema(SchemaName)
				.WithColumn("MessageId").AsGuid().NotNullable().PrimaryKey()//mta_msg_id
				.WithColumn("MtaSendId").AsInt32().NotNullable()//mta_send_internalId
				.WithColumn("RecipientTo").AsString().NotNullable()//mta_msg_rcptTo
				.WithColumn("MailFrom").AsString().NotNullable();//mta_msg_mailFrom

			Create.QuickForeignKey(SchemaName, "Messages", "MtaSend", "MtaSendId");

			Create.Table("SendMetadata")//man_mta_sendMeta
				.InSchema(SchemaName)
				.WithColumn("MtaSendId").AsInt32().NotNullable().PrimaryKey() //mta_send_internalId
				.WithColumn("Name").AsString().NotNullable()//mta_sendMeta_name
				.WithColumn("Value").AsString().NotNullable();//mta_sendMeta_value

			Create.QuickForeignKey(SchemaName, "SendMetadata", "MtaSend", "MtaSendId");

			Create.Table("TransactionStatuses")//man_mta_transactionStatus
				.InSchema(SchemaName)
				.WithLookupId("TransactionStatusId")//mta_transactionStatus_id
				.WithNameColumn();//mta_transactionStatus_name

			Create.Table("Transactions")//man_mta_transaction
				.InSchema(SchemaName)
				.WithColumn("MessageId").AsGuid().NotNullable()//mta_msg_id
				.WithColumn("IpAddressId").AsInt32().Nullable()//ip_ipAddress_id
				.WithColumn("CreatedAt").AsDateTimeOffset().NotNullable()//mta_transaction_timestamp
				.WithColumn("TransactionStatusId").AsInt32().NotNullable()//mta_transactionStatus_id
				.WithColumn("ServerResponse").AsString().NotNullable()//mta_transaction_serverResponse
				.WithColumn("ServerHostname").AsString().Nullable();//mta_transaction_serverHostname

			Create.QuickForeignKey(SchemaName, "Transactions", "TransactionStatuses", "TransactionStatusId");

			Create.QuickForeignKey(SchemaName, "Transactions", "Messages", "MessageId");

			Create.QuickForeignKey(SchemaName, "Transactions", "IpAddresses", "IpAddressId");

			Create.Table("PatternTypes")//man_rle_patternType
				.InSchema(SchemaName)
				.WithLookupId("PatternTypeId")//rle_patternType_id
				.WithNameColumn()//rle_patternType_name
				.WithDescriptionColumn();//rle_patternType_description

			Create.Table("MxPatterns")//man_rle_mxPattern
				.InSchema(SchemaName)
				.WithIdColumn("MxPatternId")//rle_mxPattern_id
				.WithNameColumn()//rle_mxPattern_name
				.WithDescriptionColumn()//rle_mxPattern_description
				.WithColumn("PatternTypeId").AsInt32().NotNullable()//rle_patternType_id
				.WithColumn("Value").AsString(250).NotNullable()//rle_mxPattern_value
				.WithColumn("IpAddressId").AsInt32().Nullable();//ip_ipAddress_id

			Create.QuickForeignKey(SchemaName, "MxPatterns", "PatternTypes", "PatternTypeId");

			Create.Table("RuleTypes")//man_rle_ruleType
				.InSchema(SchemaName)
				.WithLookupId("RuleTypeId")//rle_ruleType_id
				.WithNameColumn()//rle_ruleType_name
				.WithDescriptionColumn();//rle_ruleType_description

			Create.Table("Rules")//man_rle_rule
				.InSchema(SchemaName)
				.WithColumn("MxPatternId").AsInt32().NotNullable().PrimaryKey()//rle_mxPattern_id
				.WithColumn("RuleTypeId").AsInt32().NotNullable().PrimaryKey()//rle_ruleType_id
				.WithColumn("Value").AsString(250).NotNullable();//rle_rule_value

			Create.QuickForeignKey(SchemaName, "Rules", "MxPatterns", "MxPatternId");

			Create.QuickForeignKey(SchemaName, "Rules", "RuleTypes", "RuleTypeId");

			//Create.Index("")//IX_man_mta_sendMeta
			//	.OnTable("")//man_mta_sendMeta
			//	.WithOptions().NonClustered()
			//	.OnColumn("").Ascending();//mta_send_internalId

			//Create.Index("")//IX_man_mta_transaction
			//	.OnTable("")//man_mta_transaction
			//	.WithOptions().Clustered()
			//	.OnColumn("").Ascending();//mta_msg_id

			//Create.Index("")//BounceGroupingIndex
			//	.OnTable("")//man_mta_transaction
			//	.WithOptions().NonClustered()
			//	.OnColumn("").Ascending();//mta_transactionStatus_id

			//Create.Index("")//mta_msg_id
			//	.OnTable("")//man_mta_msg
			//	.WithOptions().NonClustered()
			//	.WithOptions().Unique()
			//	.OnColumn("");//mta_send_internalId

			//Create.Index("")//AttemptSendAfter
			//	.OnTable("")//man_mta_queue
			//	.WithOptions().NonClustered()
			//	.OnColumn("").Ascending();//mta_queue_attemptSendAfter

			//Create.Index("")//NonClusteredIndex-20141117-122722
			//	.OnTable("")//man_mta_queue
			//	.WithOptions().NonClustered()
			//	.OnColumn("").Ascending()//mta_queue_attemptSendAfter
			//	.OnColumn("");//mta_queue_isPickupLocked

			Insert.IntoTable("LocalDomains").InSchema(SchemaName)
				.WithIdentityInsert()
				.Row(new
				{
					LocalDomainId = 1,
					Domain = "localhost",
					Name = "Handle localhost messages"
				});

			Insert.IntoTable("Settings").InSchema(SchemaName)
					.Row(new { Name = "dropFolder", Value = @"c:\temp\drop\" })
					.Row(new { Name = "queueFolder", Value = @"c:\temp\queue\" })
					.Row(new { Name = "logFolder", Value = @"c:\temp\logs\" })
					.Row(new { Name = "listenPorts", Value = "25,587" })
					.Row(new { Name = "retryIntervalMinutes", Value = 5 })
					.Row(new { Name = "maxTimeInQueueMinutes", Value = 60 })
					.Row(new { Name = "defaultIpGroupId", Value = 1 })
					.Row(new { Name = "clientIdleTimeout", Value = 5 })
					.Row(new { Name = "receiveTimeout", Value = 30 })
					.Row(new { Name = "sendTimeout", Value = 30 })
					.Row(new { Name = "returnPathDomain_id", Value = 1 })
					.Row(new { Name = "maxDaysToKeepSmtpLogs", Value = 30 })
					.Row(new { Name = "eventForwardingHttpPostUrl", Value = "http://my.localhost/MantaEventHandler.ashx" })
					.Row(new { Name = "keepBounceFilesFlag", Value = false })
					.Row(new { Name = "rabbitMqEnabled", Value = true })
					.Row(new { Name = "rabbitMqUsername", Value = "guest" })
					.Row(new { Name = "rabbitMqPassword", Value = "guest" })
					.Row(new { Name = "rabbitMqHostname", Value = "localhost" });

			Insert.IntoTable("PermittedRelayIps").InSchema(SchemaName)
				.Row(new { IpAddress = "127.0.0.1", Name = "Localhost" });

			Insert.IntoTable("BounceCodes").InSchema(SchemaName)
				.Row(new { BounceCodeId = 0, Name = "Unknown" })
				.Row(new { BounceCodeId = 1, Name = "NotABounce", Description = "Not actually a bounce." })
				.Row(new { BounceCodeId = 11, Name = "BadEmailAddress" })
				.Row(new { BounceCodeId = 20, Name = "General" })
				.Row(new { BounceCodeId = 21, Name = "DnsFailure" })
				.Row(new { BounceCodeId = 22, Name = "MailboxFull" })
				.Row(new { BounceCodeId = 23, Name = "MessageSizeTooLarge" })
				.Row(new { BounceCodeId = 29, Name = "UnableToConnect" })
				.Row(new { BounceCodeId = 30, Name = "ServiceUnavailable" })
				.Row(new { BounceCodeId = 40, Name = "BounceUnknown", Description = "A bounce that we're unable to identify a reason for." })
				.Row(new { BounceCodeId = 51, Name = "KnownSpammer" })
				.Row(new { BounceCodeId = 52, Name = "SpamDetected" })
				.Row(new { BounceCodeId = 53, Name = "AttachmentDetected" })
				.Row(new { BounceCodeId = 54, Name = "RelayDenied" })
				.Row(new { BounceCodeId = 55, Name = "RateLimitedByReceivingMta" })
				.Row(new { BounceCodeId = 56, Name = "ConfigurationErrorWithSendingAddress", Description = "Indicates the receiving server reported an error with the sending address provided by Manta." })
				.Row(new { BounceCodeId = 57, Name = "PermanentlyBlockedByReceivingMta", Description = "The receiving MTA has blocked the IP address.Contact them to have it removed." })
				.Row(new { BounceCodeId = 58, Name = "TemporarilyBlockedByReceivingMta", Description = "The receiving MTA has placed a temporary block on the IP address, but will automatically remove it after a short period." });

			Insert.IntoTable("BounceRuleCriteriaTypes").InSchema(SchemaName)
				.Row(new
				{
					BounceRuleCriteriaTypeId = 0,
					Name = "Unknown",
					Description = "Default value."
				})
				.Row(new
				{
					BounceRuleCriteriaTypeId = 1,
					Name = "RegularExpressionPattern",
					Description = "The criteria is a Regex pattern to run against the message."
				})
				.Row(new
				{
					BounceRuleCriteriaTypeId = 2,
					Name = "StringMatch",
					Description = "The criteria is a string that may appear within the message."
				});

			Insert.IntoTable("BounceTypes").InSchema(SchemaName)
				.Row(new
				{
					BounceTypeid = 0,
					Name = "Unknown",
					Description = ""
				})
				.Row(new
				{
					BounceTypeid = 1,
					Name = "Hard",
					Description = "Email send attempt has failed. Do not retry."
				})
				.Row(new
				{
					BounceTypeid = 2,
					Name = "Soft",
					Description = "Email send failed, but may be accepted in the future. Retry later."
				})
				.Row(new
				{
					BounceTypeid = 3,
					Name = "Spam",
					Description = "Email send failed as the Email was identified by the receiving server as spam."
				});

			Insert.IntoTable("BounceRules").InSchema(SchemaName)
				.Row(new
				{
					BounceRuleId = 7,
					Name = "Yahoo: account doesn't exist",
					ExecutionOrder = 36,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 1,
					Criteria = @"^554\s+delivery error: dd This user doesn't have a",
					BounceTypeId = 1,
					BounceCodeId = 11
				})
				.Row(new
				{
					BounceRuleId = 6,
					Name = "Spam filtering check #1",
					ExecutionOrder = 34,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "rejected due to spam filtering",
					BounceTypeId = 3,
					BounceCodeId = 52
				})
				.Row(new
				{
					BounceRuleId = 8,
					Name = "AOL: IP address has been blocked",
					Description = "The IP address has been blocked due to a spike in unfavorable e-mail statistics.",
					ExecutionOrder = 14,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "(CON: B1)",
					BounceTypeId = 1,
					BounceCodeId = 58
				})
				.Row(new
				{
					BounceRuleId = 9,
					Name = "AOL: Blocked due to dynamic IP",
					ExecutionOrder = 15,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "(RTR: BB)",
					BounceTypeId = 1,
					BounceCodeId = 57
				})
				.Row(new
				{
					BounceRuleId = 5,
					Name = "Tesco.net(provided by Synacor) spam check",
					ExecutionOrder = 35,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "[P4] Message blocked due to spam content in the message",
					BounceTypeId = 3,
					BounceCodeId = 52
				})
				.Row(new
				{
					BounceRuleId = 10,
					Name = "AOL: Network peer is sending spam",
					ExecutionOrder = 16,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "(RTR:CH)",
					BounceTypeId = 1,
					BounceCodeId = 58
				})
				.Row(new
				{
					BounceRuleId = 11,
					Name = "AOL: Blocked as IP address not yet allocated",
					ExecutionOrder = 17,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "(RTR:BG)",
					BounceTypeId = 1,
					BounceCodeId = 57
				})
				.Row(new
				{
					BounceRuleId = 12,
					Name = "AOL: Blocked as no reverse DNS or dynamic IP",
					ExecutionOrder = 18,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "(RTR:RD)",
					BounceTypeId = 1,
					BounceCodeId = 57
				})
				.Row(new
				{
					BounceRuleId = 1,
					Name = "Yahoo: Blocked due to Spamhaus listing on PBL",
					Description = "IP is listed on Spamhaus PBL as dynamic or residential address.",
					ExecutionOrder = 1,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "5.7.1 [BL21]",
					BounceTypeId = 2,
					BounceCodeId = 51
				})
				.Row(new
				{
					BounceRuleId = 2,
					Name = "Yahoo: Blocked due to Spamhaus listing on SBL",
					Description = "IP is listed on Spamhaus SBL as a spam source.",
					ExecutionOrder = 2,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "5.7.1 [BL22]",
					BounceTypeId = 2,
					BounceCodeId = 51
				})
				.Row(new
				{
					BounceRuleId = 3,
					Name = "Yahoo: Blocked due to Spamhaus listing on XBL",
					Description = "IP is listed on Spamhaus XBL as an open proxy or spam-sending Trojan Horse.",
					ExecutionOrder = 3,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "5.7.1 [BL23]",
					BounceTypeId = 2,
					BounceCodeId = 51
				})
				.Row(new
				{
					BounceRuleId = 13,
					Name = " AOL: Blocked as excessive blocks on IP",
					ExecutionOrder = 19,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "(RTR:SC)",
					BounceTypeId = 1,
					BounceCodeId = 57
				})
				.Row(new
				{
					BounceRuleId = 14,
					Name = "AOL: Blocked as IP on Spamhaus PBL",
					ExecutionOrder = 20,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "(RTR:DU)",
					BounceTypeId = 1,
					BounceCodeId = 57
				})
				.Row(new
				{
					BounceRuleId = 15,
					Name = "AOL: Technical fault, wait 24 hours before retry",
					ExecutionOrder = 21,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "(RTR:GE)",
					BounceTypeId = 2,
					BounceCodeId = 30
				})
				.Row(new
				{
					BounceRuleId = 16,
					Name = "AOL: Permanent block on IP due to poor reputation",
					ExecutionOrder = 22,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "(RTR:BL)",
					BounceTypeId = 1,
					BounceCodeId = 57
				})
				.Row(new
				{
					BounceRuleId = 17,
					Name = "AOL: Blocked as email contained spam URL",
					ExecutionOrder = 23,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "(HVU:B1)",
					BounceTypeId = 1,
					BounceCodeId = 58
				})
				.Row(new
				{
					BounceRuleId = 18,
					Name = "AOL: Blocked as rDNS dynamic",
					ExecutionOrder = 24,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "(DNS:B1)",
					BounceTypeId = 1,
					BounceCodeId = 58
				})
				.Row(new
				{
					BounceRuleId = 19,
					Name = "AOL: Blocked as rDNS dynamic with complaints",
					ExecutionOrder = 25,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "(DNS:B2)",
					BounceTypeId = 1,
					BounceCodeId = 58
				})
				.Row(new
				{
					BounceRuleId = 20,
					Name = "AOL: Dynamic 24 hour block due to traffic",
					ExecutionOrder = 26,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "(RLY:B1)",
					BounceTypeId = 1,
					BounceCodeId = 58
				})
				.Row(new
				{
					BounceRuleId = 21,
					Name = "AOL: Hard block on IP due to reputation",
					Description = "Open support request once IP reputation improved to clear.",
					ExecutionOrder = 27,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "(RLY:B2)",
					BounceTypeId = 1,
					BounceCodeId = 57
				})
				.Row(new
				{
					BounceRuleId = 22,
					Name = "AOL: Hard block on IP due to reputation",
					Description = "Open support request once IP reputation improved to clear.",
					ExecutionOrder = 28,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "(RLY:BL)",
					BounceTypeId = 1,
					BounceCodeId = 57
				})
				.Row(new
				{
					BounceRuleId = 23,
					Name = "AOL: Hard block on email due to spam flags",
					Description = "Not a block on an IP, just on specific types of email.",
					ExecutionOrder = 29,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "(RLY:BD)",
					BounceTypeId = 1,
					BounceCodeId = 57
				})
				.Row(new
				{
					BounceRuleId = 24,
					Name = "AOL: Blocked as network peer sending spam",
					ExecutionOrder = 30,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "(RLY:CH)",
					BounceTypeId = 1,
					BounceCodeId = 58
				})
				.Row(new
				{
					BounceRuleId = 25,
					Name = "AOL: Blocked as server may be compromised",
					ExecutionOrder = 31,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "(RLY:CS4)",
					BounceTypeId = 1,
					BounceCodeId = 58
				})
				.Row(new
				{
					BounceRuleId = 26,
					Name = "AOL: Dynamic IP and high invalid recipients",
					ExecutionOrder = 32,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "(RLY: IR)",
					BounceTypeId = 1,
					BounceCodeId = 58
				})
				.Row(new
				{
					BounceRuleId = 27,
					Name = "AOL: Forwarders IP blocked",
					ExecutionOrder = 33,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "(RLY:OB)",
					BounceTypeId = 1,
					BounceCodeId = 58
				})
				.Row(new
				{
					BounceRuleId = 28,
					Name = "Yahoo: DKIM authentication failed",
					Description = "Email wasn't accepted because it failed authentication checks against the sending domain's DKIM policy.",
					ExecutionOrder = 4,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "(AU01)",
					BounceTypeId = 2,
					BounceCodeId = 56
				})
				.Row(new
				{
					BounceRuleId = 29,
					Name = "Yahoo: not accepted for policy reasons",
					Description = "Possible faked email headers or malicious content.",
					ExecutionOrder = 5,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "554 Message not allowed - [320]",
					BounceTypeId = 2,
					BounceCodeId = 56
				})
				.Row(new
				{
					BounceRuleId = 30,
					Name = "Hotmail: rejected as spam or poor IP rep",
					ExecutionOrder = 12,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "SC-001",
					BounceTypeId = 2,
					BounceCodeId = 52
				})
				.Row(new
				{
					BounceRuleId = 31,
					Name = "Hotmail: rejected due to policy reasons",
					Description = "The email server IP connecting to Outlook has exhibited namespace mining behaviour.",
					ExecutionOrder = 13,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "SC-002",
					BounceTypeId = 2,
					BounceCodeId = 56
				})
				.Row(new
				{
					BounceRuleId = 32,
					Name = "Hotmail: rejected as open relay",
					ExecutionOrder = 6,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "SC-003",
					BounceTypeId = 2,
					BounceCodeId = 56
				})
				.Row(new
				{
					BounceRuleId = 33,
					Name = "Hotmail: rejected due to complaints",
					ExecutionOrder = 7,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "SC-004",
					BounceTypeId = 2,
					BounceCodeId = 58
				})
				.Row(new
				{
					BounceRuleId = 34,
					Name = "Hotmail: rejected as from dynamic IP",
					ExecutionOrder = 8,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "DY-001",
					BounceTypeId = 2,
					BounceCodeId = 56
				})
				.Row(new
				{
					BounceRuleId = 35,
					Name = "Hotmail: rejected as may be compromised",
					ExecutionOrder = 9,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "DY-002",
					BounceTypeId = 2,
					BounceCodeId = 58
				})
				.Row(new
				{
					BounceRuleId = 36,
					Name = "Hotmail: rejected as listed on Spamhaus",
					ExecutionOrder = 10,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "OU-001",
					BounceTypeId = 2,
					BounceCodeId = 58
				})
				.Row(new
				{
					BounceRuleId = 37,
					Name = "Hotmail: rejected as spam or poor IP rep",
					ExecutionOrder = 11,
					IsBuiltIn = 1,
					BounceRuleCriteriaTypeId = 2,
					Criteria = "OU-002",
					BounceTypeId = 2,
					BounceCodeId = 58
				});

			Insert.IntoTable("EventTypes").InSchema(SchemaName)
				.Row(new { EventTypeId = 0, Name = "Unknown", Description = "" })
				.Row(new { EventTypeId = 1, Name = "Bounce", Description = "Event occurs when delivery of a message is unsuccessful." })
				.Row(new { EventTypeId = 2, Name = "Abuse", Description = "Event occurs when a feedback loop reports that someone marked the email as spam." });

			Insert.IntoTable("IpGroups").InSchema(SchemaName)
				.WithIdentityInsert()
				.Row(new
				{
					IpGroupId = 1,
					Name = "Default",
					Description = "You need at least one"
				});

			Insert.IntoTable("IpAddresses").InSchema(SchemaName)
				.WithIdentityInsert()
				.Row(new
				{
					IpAddressId = 1,
					IpAddress = "127.0.0.1",
					Hostname = "localhost",
					IsInbound = 1,
					IsOutbound = 0
				});

			Insert.IntoTable("FeedbackLoopAddresses").InSchema(SchemaName)
				.Row(new
				{
					Address = "fbl@localhost",
					Name = "Testing feedback loop address",
					Description = "Used for NUnit Tests"
				});

			Insert.IntoTable("SendStatuses").InSchema(SchemaName)
				.Row(new { SendStatusId = 1, Name = "Active", Description = "" })
				.Row(new { SendStatusId = 2, Name = "Paused", Description = "" })
				.Row(new { SendStatusId = 3, Name = "Cancelled", Description = "" });

			Insert.IntoTable("TransactionStatuses").InSchema(SchemaName)
				.Row(new { TransactionStatusId = 0, Name = "Unknown" })
				.Row(new { TransactionStatusId = 1, Name = "Deferred" })
				.Row(new { TransactionStatusId = 2, Name = "Failed" })
				.Row(new { TransactionStatusId = 3, Name = "Timed Out" })
				.Row(new { TransactionStatusId = 4, Name = "Success" })
				.Row(new { TransactionStatusId = 5, Name = "Throttled" })
				.Row(new { TransactionStatusId = 6, Name = "Discarded" });

			Insert.IntoTable("PatternTypes").InSchema(SchemaName)
				.Row(new { PatternTypeId = 1, Name = "Regex", Description = "" })
				.Row(new { PatternTypeId = 2, Name = "CommaDelimited", Description = "" });

			Insert.IntoTable("MxPatterns").InSchema(SchemaName)
				.WithIdentityInsert()
				.Row(new
				{
					MxPatternId = -1,
					Name = "DEFAULT",
					Description = "Do NOT Delete",
					PatternTypeId = 1,
					Value = "."
				});

			Insert.IntoTable("RuleTypes").InSchema(SchemaName)
				.Row(new { RuleTypeId = 1, Name = "MaxConnections", Description = "" })
				.Row(new { RuleTypeId = 2, Name = "MaxMessagesConnection", Description = "" })
				.Row(new { RuleTypeId = 3, Name = "MaxMessagesHour", Description = "" });

			Insert.IntoTable("Rules").InSchema(SchemaName)
				.Row(new { MxPatternId = -1, RuleTypeId = 1, Value = "1" })
				.Row(new { MxPatternId = -1, RuleTypeId = 2, Value = "5" })
				.Row(new { MxPatternId = -1, RuleTypeId = 3, Value = "-1" });
		}
	}
}