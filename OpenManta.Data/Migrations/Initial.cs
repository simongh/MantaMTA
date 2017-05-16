using FluentMigrator;
using FluentMigrator.Runner.Extensions;

namespace OpenManta.Data.Migrations
{
	[Migration(1)]
	public class Initial : Migration
	{
		public override void Down()
		{
			Delete.Index("IX_man_mta_sendMeta").OnTable("man_mta_sendMeta");

			Delete.Index("IX_man_mta_transaction").OnTable("man_mta_transaction");

			Delete.Index("BounceGroupingIndex").OnTable("man_mta_transaction");

			Delete.Index("mta_msg_id").OnTable("man_mta_msg");

			Delete.Index("AttemptSendAfter").OnTable("man_mta_queue");

			Delete.Index("NonClusteredIndex-20141117-122722").OnTable("man_mta_queue");

			Delete.Table("man_cfg_localDomain");

			Delete.Table("man_cfg_para");

			Delete.Table("man_cfg_relayingPermittedIp");

			Delete.Table("man_evn_bounceCode");

			Delete.Table("man_evn_bounceEvent");

			Delete.Table("man_evn_bounceRule");

			Delete.Table("man_evn_bounceRuleCriteriaType");

			Delete.Table("man_evn_bounceType");

			Delete.Table("man_evn_event");

			Delete.Table("man_evn_type");

			Delete.Table("man_ip_group");

			Delete.Table("man_ip_groupMembership");

			Delete.Table("man_ip_ipAddress");

			Delete.Table("man_mta_fblAddress");

			Delete.Table("man_mta_msg");

			Delete.Table("man_mta_queue");

			Delete.Table("man_mta_send");

			Delete.Table("man_mta_sendMeta");

			Delete.Table("man_mta_sendStatus");

			Delete.Table("man_mta_transaction");

			Delete.Table("man_mta_transactionStatus");

			Delete.Table("man_rle_mxPattern");

			Delete.Table("man_rle_patternType");

			Delete.Table("man_rle_rule");

			Delete.Table("man_rle_ruleType");
		}

		public override void Up()
		{
			Create.Table("man_cfg_localDomain")
				.WithIdColumn("cfg_localDomain_id")
				.WithColumn("cfg_localDomain_domain").AsString(255).NotNullable()
				.WithColumn("cfg_localDomain_name").AsString(50).Nullable()
				.WithColumn("cfg_localDomain_description").AsString(250).Nullable();

			Create.Table("man_cfg_para")
				.WithColumn("cfg_para_dropFolder").AsString(255).NotNullable()
				.WithColumn("cfg_para_queueFolder").AsString(255).NotNullable()
				.WithColumn("cfg_para_logFolder").AsString(255).NotNullable()
				.WithColumn("cfg_para_listenPorts").AsString(255).NotNullable()
				.WithColumn("cfg_para_retryIntervalMinutes").AsInt32().NotNullable()
				.WithColumn("cfg_para_maxTimeInQueueMinutes").AsInt32().NotNullable()
				.WithColumn("cfg_para_defaultIpGroupId").AsInt32().NotNullable()
				.WithColumn("cfg_para_clientIdleTimeout").AsInt32().NotNullable()
				.WithColumn("cfg_para_receiveTimeout").AsInt32().NotNullable()
				.WithColumn("cfg_para_sendTimeout").AsInt32().NotNullable()
				.WithColumn("cfg_para_returnPathDomain_id").AsInt32().NotNullable()
				.WithColumn("cfg_para_maxDaysToKeepSmtpLogs").AsInt32().NotNullable()
				.WithColumn("cfg_para_eventForwardingHttpPostUrl").AsString().NotNullable()
				.WithColumn("cfg_para_keepBounceFilesFlag").AsBoolean().NotNullable()
				.WithColumn("cfg_para_rabbitMqEnabled").AsBoolean().NotNullable()
				.WithColumn("cfg_para_rabbitMqUsername").AsString().NotNullable()
				.WithColumn("cfg_para_rabbitMqPassword").AsString().NotNullable()
				.WithColumn("cfg_para_rabbitMqHostname").AsString().NotNullable();

			Create.Table("man_cfg_relayingPermittedIp")
				.WithColumn("cfg_relayingPermittedIp_ip").AsString(45).NotNullable().PrimaryKey()
				.WithColumn("cfg_relayingPermittedIp_name").AsString(50).Nullable()
				.WithColumn("cfg_relayingPermittedIp_description").AsString(250).Nullable();

			Create.Table("man_evn_bounceCode")
				.WithColumn("evn_bounceCode_id").AsInt32().NotNullable()
				.WithColumn("evn_bounceCode_name").AsString(50).NotNullable()
				.WithColumn("evn_bounceCode_description").AsString(250).Nullable();

			Create.Table("man_evn_bounceEvent")
				.WithColumn("evn_event_id").AsInt32().NotNullable().PrimaryKey()
				.WithColumn("evn_bounceCode_id").AsInt32().NotNullable()
				.WithColumn("evn_bounceType_id").AsInt32().NotNullable()
				.WithColumn("evn_bounceEvent_message").AsString().NotNullable();

			Create.Table("man_evn_bounceRule")
				.WithColumn("evn_bounceRule_id").AsInt32().NotNullable()
				.WithColumn("evn_bounceRule_name").AsString(50).NotNullable()
				.WithColumn("evn_bounceRule_description").AsString(250).Nullable()
				.WithColumn("evn_bounceRule_executionOrder").AsInt32().NotNullable()
				.WithColumn("evn_bounceRule_isBuiltIn").AsBoolean().NotNullable()
				.WithColumn("evn_bounceRuleCriteriaType_id").AsInt32().NotNullable()
				.WithColumn("evn_bounceRule_criteria").AsString().NotNullable()
				.WithColumn("evn_bounceRule_mantaBounceType").AsInt32().NotNullable()
				.WithColumn("evn_bounceRule_mantaBounceCode").AsInt32().NotNullable();

			Create.Table("man_evn_bounceRuleCriteriaType")
				.WithColumn("evn_bounceRuleCriteriaType_id").AsInt32().NotNullable()
				.WithColumn("evn_bounceRuleCriteriaType_name").AsString(50).NotNullable()
				.WithColumn("evn_bounceRuleCriteriaType_description").AsString(250).Nullable();

			Create.Table("man_evn_bounceType")
				.WithColumn("evn_bounceType_id").AsInt32().NotNullable()
				.WithColumn("evn_bounceType_name").AsString(50).NotNullable()
				.WithColumn("evn_bounceType_description").AsString(250).Nullable();

			Create.Table("man_evn_event")
				.WithIdColumn("evn_event_id")
				.WithColumn("evn_type_id").AsInt32().NotNullable()
				.WithColumn("evn_event_timestamp").AsDateTime().NotNullable()
				.WithColumn("evn_event_emailAddress").AsString(320).NotNullable()
				.WithColumn("snd_send_id").AsString(20).NotNullable()
				.WithColumn("evn_event_forwarded").AsBoolean().NotNullable().WithDefaultValue(0);

			Create.Table("man_evn_type")
				.WithColumn("evn_type_id").AsInt32().NotNullable()
				.WithColumn("evn_type_name").AsString(50).NotNullable()
				.WithColumn("evn_type_description").AsString(250).Nullable();

			Create.Table("man_ip_group")
				.WithIdColumn("ip_group_id")
				.WithColumn("ip_group_name").AsString(50).NotNullable()
				.WithColumn("ip_group_description").AsString(250).Nullable();

			Create.Table("man_ip_groupMembership")
				.WithColumn("ip_group_id").AsInt32().NotNullable().PrimaryKey()
				.WithColumn("ip_ipAddress_id").AsInt32().NotNullable().PrimaryKey();

			Create.Table("man_ip_ipAddress")
				.WithIdColumn("ip_ipAddress_id")
				.WithColumn("ip_ipAddress_ipAddress").AsAnsiString(45).NotNullable()
				.WithColumn("ip_ipAddress_hostname").AsAnsiString(255).Nullable()
				.WithColumn("ip_ipAddress_isInbound").AsBoolean().Nullable()
				.WithColumn("ip_ipAddress_isOutbound").AsBoolean().Nullable();

			Create.Table("man_mta_fblAddress")
				.WithColumn("mta_fblAddress_address").AsString(320).NotNullable().PrimaryKey()
				.WithColumn("mta_fblAddress_name").AsString(50).NotNullable()
				.WithColumn("mta_fblAddress_description").AsString(250).Nullable();

			Create.Table("man_mta_msg")
				.WithColumn("mta_msg_id").AsGuid().NotNullable().PrimaryKey()
				.WithColumn("mta_send_internalId").AsInt32().NotNullable()
				.WithColumn("mta_msg_rcptTo").AsString().NotNullable()
				.WithColumn("mta_msg_mailFrom").AsString().NotNullable();

			Create.Table("man_mta_queue")
				.WithColumn("mta_msg_id").AsGuid().NotNullable().PrimaryKey()
				.WithColumn("mta_queue_queuedTimestamp").AsDateTime().NotNullable()
				.WithColumn("mta_queue_attemptSendAfter").AsDateTime().NotNullable()
				.WithColumn("mta_queue_isPickupLocked").AsBoolean().NotNullable()
				.WithColumn("mta_queue_dataPath").AsString().NotNullable()
				.WithColumn("ip_group_id").AsInt32().NotNullable()
				.WithColumn("mta_send_internalId").AsInt32().NotNullable();

			Create.Table("man_mta_send")
				.WithColumn("mta_send_internalId").AsInt32().NotNullable().PrimaryKey()
				.WithColumn("mta_send_id").AsString(20).NotNullable()
				.WithColumn("mta_sendStatus_id").AsInt32().NotNullable()
				.WithColumn("mta_send_createdTimestamp").AsDateTime().NotNullable()
				.WithColumn("mta_send_messages").AsInt32().NotNullable().WithDefaultValue(0)
				.WithColumn("mta_send_accepted").AsInt32().NotNullable().WithDefaultValue(0)
				.WithColumn("mta_send_rejected").AsInt32().NotNullable().WithDefaultValue(0);

			Create.Table("man_mta_sendMeta")
				.WithColumn("mta_send_internalId").AsInt32().NotNullable()
				.WithColumn("mta_sendMeta_name").AsString().NotNullable()
				.WithColumn("mta_sendMeta_value").AsString().NotNullable();

			Create.Table("man_mta_sendStatus")
				.WithColumn("mta_sendStatus_id").AsInt32().NotNullable()
				.WithColumn("mta_sendStatus_name").AsString(50).NotNullable()
				.WithColumn("mta_sendStatus_description").AsString(250).Nullable();

			Create.Table("man_mta_transaction")
				.WithColumn("mta_msg_id").AsGuid().NotNullable()
				.WithColumn("ip_ipAddress_id").AsInt32().Nullable()
				.WithColumn("mta_transaction_timestamp").AsDateTime().NotNullable()
				.WithColumn("mta_transactionStatus_id").AsInt32().NotNullable()
				.WithColumn("mta_transaction_serverResponse").AsString().NotNullable()
				.WithColumn("mta_transaction_serverHostname").AsString().Nullable();

			Create.Table("man_mta_transactionStatus")
				.WithColumn("mta_transactionStatus_id").AsInt32().NotNullable()
				.WithColumn("mta_transactionStatus_name").AsString(50).NotNullable();

			Create.Table("man_rle_mxPattern")
				.WithIdColumn("rle_mxPattern_id")
				.WithColumn("rle_mxPattern_name").AsString(50).NotNullable()
				.WithColumn("rle_mxPattern_description").AsString(250).Nullable()
				.WithColumn("rle_patternType_id").AsInt32().NotNullable()
				.WithColumn("rle_mxPattern_value").AsString(250).NotNullable()
				.WithColumn("ip_ipAddress_id").AsInt32().Nullable();

			Create.Table("man_rle_patternType")
				.WithColumn("rle_patternType_id").AsInt32().NotNullable()
				.WithColumn("rle_patternType_name").AsString(50).NotNullable()
				.WithColumn("rle_patternType_description").AsString(250).Nullable();

			Create.Table("man_rle_rule")
				.WithColumn("rle_mxPattern_id").AsInt32().NotNullable().PrimaryKey()
				.WithColumn("rle_ruleType_id").AsInt32().NotNullable().PrimaryKey()
				.WithColumn("rle_rule_value").AsString(250).NotNullable();

			Create.Table("man_rle_ruleType")
				.WithColumn("rle_ruleType_id").AsInt32().NotNullable()
				.WithColumn("rle_ruleType_name").AsString(50).NotNullable()
				.WithColumn("rle_ruleType_description").AsString(250).Nullable();

			Create.Index("IX_man_mta_sendMeta")
				.OnTable("man_mta_sendMeta")
				.WithOptions().NonClustered()
				.OnColumn("mta_send_internalId").Ascending();

			Create.Index("IX_man_mta_transaction")
				.OnTable("man_mta_transaction")
				.WithOptions().Clustered()
				.OnColumn("mta_msg_id").Ascending();

			Create.Index("BounceGroupingIndex")
				.OnTable("man_mta_transaction")
				.WithOptions().NonClustered()
				.OnColumn("mta_transactionStatus_id").Ascending();

			Create.Index("mta_msg_id")
				.OnTable("man_mta_msg")
				.WithOptions().NonClustered()
				.WithOptions().Unique()
				.OnColumn("mta_send_internalId");

			Create.Index("AttemptSendAfter")
				.OnTable("man_mta_queue")
				.WithOptions().NonClustered()
				.OnColumn("mta_queue_attemptSendAfter").Ascending();

			Create.Index("NonClusteredIndex-20141117-122722")
				.OnTable("man_mta_queue")
				.WithOptions().NonClustered()
				.OnColumn("mta_queue_attemptSendAfter").Ascending()
				.OnColumn("mta_queue_isPickupLocked");

			Insert.IntoTable("man_cfg_localDomain")
				.WithIdentityInsert()
				.Row(new
				{
					cfg_localDomain_id = 1,
					cfg_localDomain_domain = "localhost",
					cfg_localDomain_name = "Handle localhost messages"
				});

			Insert.IntoTable("man_cfg_para")
				.Row(new
				{
					cfg_para_dropFolder = @"c:\temp\drop\",
					cfg_para_queueFolder = @"c:\temp\queue\",
					cfg_para_logFolder = @"c:\temp\logs\",
					cfg_para_listenPorts = "25,587",
					cfg_para_retryIntervalMinutes = 5,
					cfg_para_maxTimeInQueueMinutes = 60,
					cfg_para_defaultIpGroupId = 1,
					cfg_para_clientIdleTimeout = 5,
					cfg_para_receiveTimeout = 30,
					cfg_para_sendTimeout = 30,
					cfg_para_returnPathDomain_id = 1,
					cfg_para_maxDaysToKeepSmtpLogs = 30,
					cfg_para_eventForwardingHttpPostUrl = "http://my.localhost/MantaEventHandler.ashx",
					cfg_para_keepBounceFilesFlag = false,
					cfg_para_rabbitMqEnabled = true,
					cfg_para_rabbitMqUsername = "guest",
					cfg_para_rabbitMqPassword = "guest",
					cfg_para_rabbitMqHostname = "localhost"
				});

			Insert.IntoTable("man_cfg_relayingPermittedIp")
				.Row(new
				{
					cfg_relayingPermittedIp_ip = "127.0.0.1",
					cfg_relayingPermittedIp_name = "Localhost"
				});

			Insert.IntoTable("man_evn_bounceCode")
				.Row(new { evn_bounceCode_id = 0, evn_bounceCode_name = "Unknown" })
				.Row(new { evn_bounceCode_id = 1, evn_bounceCode_name = "NotABounce", evn_bounceCode_description = "Not actually a bounce." })
				.Row(new { evn_bounceCode_id = 11, evn_bounceCode_name = "BadEmailAddress" })
				.Row(new { evn_bounceCode_id = 20, evn_bounceCode_name = "General" })
				.Row(new { evn_bounceCode_id = 21, evn_bounceCode_name = "DnsFailure" })
				.Row(new { evn_bounceCode_id = 22, evn_bounceCode_name = "MailboxFull" })
				.Row(new { evn_bounceCode_id = 23, evn_bounceCode_name = "MessageSizeTooLarge" })
				.Row(new { evn_bounceCode_id = 29, evn_bounceCode_name = "UnableToConnect" })
				.Row(new { evn_bounceCode_id = 30, evn_bounceCode_name = "ServiceUnavailable" })
				.Row(new { evn_bounceCode_id = 40, evn_bounceCode_name = "BounceUnknown", evn_bounceCode_description = "A bounce that we're unable to identify a reason for." })
				.Row(new { evn_bounceCode_id = 51, evn_bounceCode_name = "KnownSpammer" })
				.Row(new { evn_bounceCode_id = 52, evn_bounceCode_name = "SpamDetected" })
				.Row(new { evn_bounceCode_id = 53, evn_bounceCode_name = "AttachmentDetected" })
				.Row(new { evn_bounceCode_id = 54, evn_bounceCode_name = "RelayDenied" })
				.Row(new { evn_bounceCode_id = 55, evn_bounceCode_name = "RateLimitedByReceivingMta" })
				.Row(new
				{
					evn_bounceCode_id = 56,
					evn_bounceCode_name = "ConfigurationErrorWithSendingAddress",
					evn_bounceCode_description = "Indicates the receiving server reported an error with the sending address provided by Manta."
				})
				.Row(new
				{
					evn_bounceCode_id = 57,
					evn_bounceCode_name = "PermanentlyBlockedByReceivingMta",
					evn_bounceCode_description = "The receiving MTA has blocked the IP address.Contact them to have it removed."
				})
				.Row(new
				{
					evn_bounceCode_id = 58,
					evn_bounceCode_name = "TemporarilyBlockedByReceivingMta",
					evn_bounceCode_description = "The receiving MTA has placed a temporary block on the IP address, but will automatically remove it after a short period."
				});

			Insert.IntoTable("man_evn_bounceRule")
				.Row(new
				{
					evn_bounceRule_id = 7,
					evn_bounceRule_name = "Yahoo: account doesn't exist",
					evn_bounceRule_executionOrder = 36,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 1,
					evn_bounceRule_criteria = @"^554\s+delivery error: dd This user doesn't have a",
					evn_bounceRule_mantaBounceType = 1,
					evn_bounceRule_mantaBounceCode = 11
				})
				.Row(new
				{
					evn_bounceRule_id = 6,
					evn_bounceRule_name = "Spam filtering check #1",
					evn_bounceRule_executionOrder = 34,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "rejected due to spam filtering",
					evn_bounceRule_mantaBounceType = 3,
					evn_bounceRule_mantaBounceCode = 52
				})
				.Row(new
				{
					evn_bounceRule_id = 8,
					evn_bounceRule_name = "AOL: IP address has been blocked",
					evn_bounceRule_description = "The IP address has been blocked due to a spike in unfavorable e-mail statistics.",
					evn_bounceRule_executionOrder = 14,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "(CON: B1)",
					evn_bounceRule_mantaBounceType = 1,
					evn_bounceRule_mantaBounceCode = 58
				})
				.Row(new
				{
					evn_bounceRule_id = 9,
					evn_bounceRule_name = "AOL: Blocked due to dynamic IP",
					evn_bounceRule_executionOrder = 15,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "(RTR: BB)",
					evn_bounceRule_mantaBounceType = 1,
					evn_bounceRule_mantaBounceCode = 57
				})
				.Row(new
				{
					evn_bounceRule_id = 5,
					evn_bounceRule_name = "Tesco.net(provided by Synacor) spam check",
					evn_bounceRule_executionOrder = 35,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "[P4] Message blocked due to spam content in the message",
					evn_bounceRule_mantaBounceType = 3,
					evn_bounceRule_mantaBounceCode = 52
				})
				.Row(new
				{
					evn_bounceRule_id = 10,
					evn_bounceRule_name = "AOL: Network peer is sending spam",
					evn_bounceRule_executionOrder = 16,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "(RTR:CH)",
					evn_bounceRule_mantaBounceType = 1,
					evn_bounceRule_mantaBounceCode = 58
				})
				.Row(new
				{
					evn_bounceRule_id = 11,
					evn_bounceRule_name = "AOL: Blocked as IP address not yet allocated",
					evn_bounceRule_executionOrder = 17,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "(RTR:BG)",
					evn_bounceRule_mantaBounceType = 1,
					evn_bounceRule_mantaBounceCode = 57
				})
				.Row(new
				{
					evn_bounceRule_id = 12,
					evn_bounceRule_name = "AOL: Blocked as no reverse DNS or dynamic IP",
					evn_bounceRule_executionOrder = 18,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "(RTR:RD)",
					evn_bounceRule_mantaBounceType = 1,
					evn_bounceRule_mantaBounceCode = 57
				})
				.Row(new
				{
					evn_bounceRule_id = 1,
					evn_bounceRule_name = "Yahoo: Blocked due to Spamhaus listing on PBL",
					evn_bounceRule_description = "IP is listed on Spamhaus PBL as dynamic or residential address.",
					evn_bounceRule_executionOrder = 1,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "5.7.1 [BL21]",
					evn_bounceRule_mantaBounceType = 2,
					evn_bounceRule_mantaBounceCode = 51
				})
				.Row(new
				{
					evn_bounceRule_id = 2,
					evn_bounceRule_name = "Yahoo: Blocked due to Spamhaus listing on SBL",
					evn_bounceRule_description = "IP is listed on Spamhaus SBL as a spam source.",
					evn_bounceRule_executionOrder = 2,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "5.7.1 [BL22]",
					evn_bounceRule_mantaBounceType = 2,
					evn_bounceRule_mantaBounceCode = 51
				})
				.Row(new
				{
					evn_bounceRule_id = 3,
					evn_bounceRule_name = "Yahoo: Blocked due to Spamhaus listing on XBL",
					evn_bounceRule_description = "IP is listed on Spamhaus XBL as an open proxy or spam-sending Trojan Horse.",
					evn_bounceRule_executionOrder = 3,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "5.7.1 [BL23]",
					evn_bounceRule_mantaBounceType = 2,
					evn_bounceRule_mantaBounceCode = 51
				})
				.Row(new
				{
					evn_bounceRule_id = 13,
					evn_bounceRule_name = " AOL: Blocked as excessive blocks on IP",
					evn_bounceRule_executionOrder = 19,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "(RTR:SC)",
					evn_bounceRule_mantaBounceType = 1,
					evn_bounceRule_mantaBounceCode = 57
				})
				.Row(new
				{
					evn_bounceRule_id = 14,
					evn_bounceRule_name = "AOL: Blocked as IP on Spamhaus PBL",
					evn_bounceRule_executionOrder = 20,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "(RTR:DU)",
					evn_bounceRule_mantaBounceType = 1,
					evn_bounceRule_mantaBounceCode = 57
				})
				.Row(new
				{
					evn_bounceRule_id = 15,
					evn_bounceRule_name = "AOL: Technical fault, wait 24 hours before retry",
					evn_bounceRule_executionOrder = 21,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "(RTR:GE)",
					evn_bounceRule_mantaBounceType = 2,
					evn_bounceRule_mantaBounceCode = 30
				})
				.Row(new
				{
					evn_bounceRule_id = 16,
					evn_bounceRule_name = "AOL: Permanent block on IP due to poor reputation",
					evn_bounceRule_executionOrder = 22,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "(RTR:BL)",
					evn_bounceRule_mantaBounceType = 1,
					evn_bounceRule_mantaBounceCode = 57
				})
				.Row(new
				{
					evn_bounceRule_id = 17,
					evn_bounceRule_name = "AOL: Blocked as email contained spam URL",
					evn_bounceRule_executionOrder = 23,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "(HVU:B1)",
					evn_bounceRule_mantaBounceType = 1,
					evn_bounceRule_mantaBounceCode = 58
				})
				.Row(new
				{
					evn_bounceRule_id = 18,
					evn_bounceRule_name = "AOL: Blocked as rDNS dynamic",
					evn_bounceRule_executionOrder = 24,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "(DNS:B1)",
					evn_bounceRule_mantaBounceType = 1,
					evn_bounceRule_mantaBounceCode = 58
				})
				.Row(new
				{
					evn_bounceRule_id = 19,
					evn_bounceRule_name = "AOL: Blocked as rDNS dynamic with complaints",
					evn_bounceRule_executionOrder = 25,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "(DNS:B2)",
					evn_bounceRule_mantaBounceType = 1,
					evn_bounceRule_mantaBounceCode = 58
				})
				.Row(new
				{
					evn_bounceRule_id = 20,
					evn_bounceRule_name = "AOL: Dynamic 24 hour block due to traffic",
					evn_bounceRule_executionOrder = 26,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "(RLY:B1)",
					evn_bounceRule_mantaBounceType = 1,
					evn_bounceRule_mantaBounceCode = 58
				})
				.Row(new
				{
					evn_bounceRule_id = 21,
					evn_bounceRule_name = "AOL: Hard block on IP due to reputation",
					evn_bounceRule_description = "Open support request once IP reputation improved to clear.",
					evn_bounceRule_executionOrder = 27,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "(RLY:B2)",
					evn_bounceRule_mantaBounceType = 1,
					evn_bounceRule_mantaBounceCode = 57
				})
				.Row(new
				{
					evn_bounceRule_id = 22,
					evn_bounceRule_name = "AOL: Hard block on IP due to reputation",
					evn_bounceRule_description = "Open support request once IP reputation improved to clear.",
					evn_bounceRule_executionOrder = 28,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "(RLY:BL)",
					evn_bounceRule_mantaBounceType = 1,
					evn_bounceRule_mantaBounceCode = 57
				})
				.Row(new
				{
					evn_bounceRule_id = 23,
					evn_bounceRule_name = "AOL: Hard block on email due to spam flags",
					evn_bounceRule_description = "Not a block on an IP, just on specific types of email.",
					evn_bounceRule_executionOrder = 29,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "(RLY:BD)",
					evn_bounceRule_mantaBounceType = 1,
					evn_bounceRule_mantaBounceCode = 57
				})
				.Row(new
				{
					evn_bounceRule_id = 24,
					evn_bounceRule_name = "AOL: Blocked as network peer sending spam",
					evn_bounceRule_executionOrder = 30,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "(RLY:CH)",
					evn_bounceRule_mantaBounceType = 1,
					evn_bounceRule_mantaBounceCode = 58
				})
				.Row(new
				{
					evn_bounceRule_id = 25,
					evn_bounceRule_name = "AOL: Blocked as server may be compromised",
					evn_bounceRule_executionOrder = 31,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "(RLY:CS4)",
					evn_bounceRule_mantaBounceType = 1,
					evn_bounceRule_mantaBounceCode = 58
				})
				.Row(new
				{
					evn_bounceRule_id = 26,
					evn_bounceRule_name = "AOL: Dynamic IP and high invalid recipients",
					evn_bounceRule_executionOrder = 32,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "(RLY: IR)",
					evn_bounceRule_mantaBounceType = 1,
					evn_bounceRule_mantaBounceCode = 58
				})
				.Row(new
				{
					evn_bounceRule_id = 27,
					evn_bounceRule_name = "AOL: Forwarders IP blocked",
					evn_bounceRule_executionOrder = 33,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "(RLY:OB)",
					evn_bounceRule_mantaBounceType = 1,
					evn_bounceRule_mantaBounceCode = 58
				})
				.Row(new
				{
					evn_bounceRule_id = 28,
					evn_bounceRule_name = "Yahoo: DKIM authentication failed",
					evn_bounceRule_description = "Email wasn't accepted because it failed authentication checks against the sending domain's DKIM policy.",
					evn_bounceRule_executionOrder = 4,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "(AU01)",
					evn_bounceRule_mantaBounceType = 2,
					evn_bounceRule_mantaBounceCode = 56
				})
				.Row(new
				{
					evn_bounceRule_id = 29,
					evn_bounceRule_name = "Yahoo: not accepted for policy reasons",
					evn_bounceRule_description = "Possible faked email headers or malicious content.",
					evn_bounceRule_executionOrder = 5,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "554 Message not allowed - [320]",
					evn_bounceRule_mantaBounceType = 2,
					evn_bounceRule_mantaBounceCode = 56
				})
				.Row(new
				{
					evn_bounceRule_id = 30,
					evn_bounceRule_name = "Hotmail: rejected as spam or poor IP rep",
					evn_bounceRule_executionOrder = 12,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "SC-001",
					evn_bounceRule_mantaBounceType = 2,
					evn_bounceRule_mantaBounceCode = 52
				})
				.Row(new
				{
					evn_bounceRule_id = 31,
					evn_bounceRule_name = "Hotmail: rejected due to policy reasons",
					evn_bounceRule_description = "The email server IP connecting to Outlook has exhibited namespace mining behaviour.",
					evn_bounceRule_executionOrder = 13,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "SC-002",
					evn_bounceRule_mantaBounceType = 2,
					evn_bounceRule_mantaBounceCode = 56
				})
				.Row(new
				{
					evn_bounceRule_id = 32,
					evn_bounceRule_name = "Hotmail: rejected as open relay",
					evn_bounceRule_executionOrder = 6,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "SC-003",
					evn_bounceRule_mantaBounceType = 2,
					evn_bounceRule_mantaBounceCode = 56
				})
				.Row(new
				{
					evn_bounceRule_id = 33,
					evn_bounceRule_name = "Hotmail: rejected due to complaints",
					evn_bounceRule_executionOrder = 7,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "SC-004",
					evn_bounceRule_mantaBounceType = 2,
					evn_bounceRule_mantaBounceCode = 58
				})
				.Row(new
				{
					evn_bounceRule_id = 24,
					evn_bounceRule_name = "Hotmail: rejected as from dynamic IP",
					evn_bounceRule_executionOrder = 8,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "DY-001",
					evn_bounceRule_mantaBounceType = 2,
					evn_bounceRule_mantaBounceCode = 56
				})
				.Row(new
				{
					evn_bounceRule_id = 25,
					evn_bounceRule_name = "Hotmail: rejected as may be compromised",
					evn_bounceRule_executionOrder = 9,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "DY-002",
					evn_bounceRule_mantaBounceType = 2,
					evn_bounceRule_mantaBounceCode = 58
				})
				.Row(new
				{
					evn_bounceRule_id = 26,
					evn_bounceRule_name = "Hotmail: rejected as listed on Spamhaus",
					evn_bounceRule_executionOrder = 10,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "OU-001",
					evn_bounceRule_mantaBounceType = 2,
					evn_bounceRule_mantaBounceCode = 58
				})
				.Row(new
				{
					evn_bounceRule_id = 27,
					evn_bounceRule_name = "Hotmail: rejected as spam or poor IP rep",
					evn_bounceRule_executionOrder = 11,
					evn_bounceRule_isBuiltIn = 1,
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRule_criteria = "OU-002",
					evn_bounceRule_mantaBounceType = 2,
					evn_bounceRule_mantaBounceCode = 58
				});

			Insert.IntoTable("man_evn_bounceRuleCriteriaType")
				.Row(new
				{
					evn_bounceRuleCriteriaType_id = 0,
					evn_bounceRuleCriteriaType_name = "Unknown",
					evn_bounceRuleCriteriaType_description = "Default value."
				})
				.Row(new
				{
					evn_bounceRuleCriteriaType_id = 1,
					evn_bounceRuleCriteriaType_name = "RegularExpressionPattern",
					evn_bounceRuleCriteriaType_description = "The criteria is a Regex pattern to run against the message."
				})
				.Row(new
				{
					evn_bounceRuleCriteriaType_id = 2,
					evn_bounceRuleCriteriaType_name = "StringMatch",
					evn_bounceRuleCriteriaType_description = "The criteria is a string that may appear within the message."
				});

			Insert.IntoTable("man_evn_bounceType")
				.Row(new
				{
					evn_bounceType_id = 0,
					evn_bounceType_name = "Unknown",
					evn_bounceType_description = ""
				})
				.Row(new
				{
					evn_bounceType_id = 1,
					evn_bounceType_name = "Hard",
					evn_bounceType_description = "Email send attempt has failed. Do not retry."
				})
				.Row(new
				{
					evn_bounceType_id = 2,
					evn_bounceType_name = "Soft",
					evn_bounceType_description = "Email send failed, but may be accepted in the future. Retry later."
				})
				.Row(new
				{
					evn_bounceType_id = 3,
					evn_bounceType_name = "Spam",
					evn_bounceType_description = "Email send failed as the Email was identified by the receiving server as spam."
				});

			Insert.IntoTable("man_evn_type")
				.Row(new
				{
					evn_type_id = 0,
					evn_type_name = "Unknown",
					evn_type_description = ""
				})
				.Row(new
				{
					evn_type_id = 1,
					evn_type_name = "Bounce",
					evn_type_description = "Event occurs when delivery of a message is unsuccessful."
				})
				.Row(new
				{
					evn_type_id = 2,
					evn_type_name = "Abuse",
					evn_type_description = "Event occurs when a feedback loop reports that someone marked the email as spam."
				});

			Insert.IntoTable("man_ip_group")
				.WithIdentityInsert()
				.Row(new
				{
					ip_group_id = 1,
					ip_group_name = "Default",
					ip_group_description = "You need at least one"
				});

			Insert.IntoTable("man_ip_ipAddress")
				.WithIdentityInsert()
				.Row(new
				{
					ip_ipAddress_id = 1,
					ip_ipAddress_ipAddress = "127.0.0.1",
					ip_ipAddress_hostname = "localhost",
					ip_ipAddress_isInbound = 1,
					ip_ipAddress_isOutbound = 0
				});

			Insert.IntoTable("man_mta_fblAddress")
				.Row(new
				{
					mta_fblAddress_address = "fbl@localhost",
					mta_fblAddress_name = "Testing feedback loop address",
					mta_fblAddress_description = "Used for NUnit Tests"
				});

			Insert.IntoTable("man_mta_sendStatus")
				.Row(new
				{
					mta_sendStatus_id = 1,
					mta_sendStatus_name = "Active",
					mta_sendStatus_description = ""
				})
				.Row(new
				{
					mta_sendStatus_id = 2,
					mta_sendStatus_name = "Paused",
					mta_sendStatus_description = ""
				})
				.Row(new
				{
					mta_sendStatus_id = 3,
					mta_sendStatus_name = "Cancelled",
					mta_sendStatus_description = ""
				});

			Insert.IntoTable("man_mta_transactionStatus")
				.Row(new
				{
					mta_transactionStatus_id = 0,
					mta_transactionStatus_name = "Unknown"
				})
				.Row(new
				{
					mta_transactionStatus_id = 1,
					mta_transactionStatus_name = "Deferred"
				})
				.Row(new
				{
					mta_transactionStatus_id = 2,
					mta_transactionStatus_name = "Failed"
				})
				.Row(new
				{
					mta_transactionStatus_id = 3,
					mta_transactionStatus_name = "Timed Out"
				})
				.Row(new
				{
					mta_transactionStatus_id = 4,
					mta_transactionStatus_name = "Success"
				})
				.Row(new
				{
					mta_transactionStatus_id = 5,
					mta_transactionStatus_name = "Throttled"
				})
				.Row(new
				{
					mta_transactionStatus_id = 6,
					mta_transactionStatus_name = "Discarded"
				});

			Insert.IntoTable("man_rle_mxPattern")
				.WithIdentityInsert()
				.Row(new
				{
					rle_mxPattern_id = -1,
					rle_mxPattern_name = "DEFAULT",
					rle_mxPattern_description = "Do NOT Delete",
					rle_patternType_id = 1,
					rle_mxPattern_value = "."
				});

			Insert.IntoTable("man_rle_patternType")
				.Row(new
				{
					rle_patternType_id = 1,
					rle_patternType_name = "Regex",
					rle_patternType_description = ""
				})
				.Row(new
				{
					rle_patternType_id = 2,
					rle_patternType_name = "CommaDelimited",
					rle_patternType_description = ""
				});

			Insert.IntoTable("man_rle_rule")
				.Row(new
				{
					rle_mxPattern_id = -1,
					rle_ruleType_id = 1,
					rle_rule_value = "1"
				})
				.Row(new
				{
					rle_mxPattern_id = -1,
					rle_ruleType_id = 2,
					rle_rule_value = "5"
				})
				.Row(new
				{
					rle_mxPattern_id = -1,
					rle_ruleType_id = 3,
					rle_rule_value = "-1"
				});

			Insert.IntoTable("man_rle_ruleType")
				.Row(new
				{
					rle_ruleType_id = 1,
					rle_ruleType_name = "MaxConnections",
					rle_ruleType_description = ""
				})
				.Row(new
				{
					rle_ruleType_id = 2,
					rle_ruleType_name = "MaxMessagesConnection",
					rle_ruleType_description = ""
				})
				.Row(new
				{
					rle_ruleType_id = 3,
					rle_ruleType_name = "MaxMessagesHour",
					rle_ruleType_description = ""
				});
		}
	}
}