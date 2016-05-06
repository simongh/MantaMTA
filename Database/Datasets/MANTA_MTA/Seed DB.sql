/*
Sql seeds initial data to the database.
Replace C:\Users\MyUser\Documents\Projects\MantaMTA to your project path
*/

BULK INSERT dbo.man_cfg_localDomain 
   FROM 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_cfg_localDomain.txt' 
   WITH (FORMATFILE = 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_cfg_localDomain.xml');
GO

BULK INSERT dbo.man_cfg_para 
   FROM 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_cfg_para.txt' 
   WITH (FORMATFILE = 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_cfg_para.xml');
GO

BULK INSERT dbo.man_cfg_relayingPermittedIp 
   FROM 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_cfg_relayingPermittedIp.txt' 
   WITH (FORMATFILE = 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_cfg_relayingPermittedIp.xml');
GO

BULK INSERT dbo.man_evn_bounceCode 
   FROM 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_evn_bounceCode.txt' 
   WITH (FORMATFILE = 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_evn_bounceCode.xml');
GO

BULK INSERT dbo.man_evn_bounceEvent 
   FROM 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_evn_bounceEvent.txt' 
   WITH (FORMATFILE = 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_evn_bounceEvent.xml');
GO

BULK INSERT dbo.man_evn_bounceRule 
   FROM 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_evn_bounceRule.txt' 
   WITH (FORMATFILE = 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_evn_bounceRule.xml');
GO

BULK INSERT dbo.man_evn_bounceRuleCriteriaType 
   FROM 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_evn_bounceRuleCriteriaType.txt' 
   WITH (FORMATFILE = 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_evn_bounceRuleCriteriaType.xml');
GO

BULK INSERT dbo.man_evn_bounceType 
   FROM 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_evn_bounceType.txt' 
   WITH (FORMATFILE = 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_evn_bounceType.xml');
GO

BULK INSERT dbo.man_evn_event 
   FROM 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_evn_event.txt' 
   WITH (FORMATFILE = 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_evn_event.xml');
GO

BULK INSERT dbo.man_evn_type 
   FROM 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_evn_type.txt' 
   WITH (FORMATFILE = 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_evn_type.xml');
GO

BULK INSERT dbo.man_ip_group 
   FROM 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_ip_group.txt' 
   WITH (FORMATFILE = 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_ip_group.xml');
GO


BULK INSERT dbo.man_ip_groupMembership 
   FROM 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_ip_groupMembership.txt' 
   WITH (FORMATFILE = 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_ip_groupMembership.xml');
GO

BULK INSERT dbo.man_ip_ipAddress 
   FROM 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_ip_ipAddress.txt' 
   WITH (FORMATFILE = 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_ip_ipAddress.xml');
GO

BULK INSERT dbo.man_mta_fblAddress 
   FROM 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_mta_fblAddress.txt' 
   WITH (FORMATFILE = 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_mta_fblAddress.xml');
GO

BULK INSERT dbo.man_mta_msg 
   FROM 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_mta_msg.txt' 
   WITH (FORMATFILE = 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_mta_msg.xml');
GO

BULK INSERT dbo.man_mta_queue 
   FROM 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_mta_queue.txt' 
   WITH (FORMATFILE = 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_mta_queue.xml');
GO

BULK INSERT dbo.man_mta_send 
   FROM 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_mta_send.txt' 
   WITH (FORMATFILE = 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_mta_send.xml');
GO

BULK INSERT dbo.man_mta_sendStatus 
   FROM 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_mta_sendStatus.txt' 
   WITH (FORMATFILE = 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_mta_sendStatus.xml');
GO

BULK INSERT dbo.man_mta_transaction 
   FROM 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_mta_transaction.txt' 
   WITH (FORMATFILE = 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_mta_transaction.xml');
GO

BULK INSERT dbo.man_mta_transactionStatus 
   FROM 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_mta_transactionStatus.txt' 
   WITH (FORMATFILE = 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_mta_transactionStatus.xml');
GO

BULK INSERT dbo.man_rle_mxPattern 
   FROM 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_rle_mxPattern.txt' 
   WITH (FORMATFILE = 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_rle_mxPattern.xml');
GO

BULK INSERT dbo.man_rle_patternType 
   FROM 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_rle_patternType.txt' 
   WITH (FORMATFILE = 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_rle_patternType.xml');
GO

BULK INSERT dbo.man_rle_rule 
   FROM 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_rle_rule.txt' 
   WITH (FORMATFILE = 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_rle_rule.xml');
GO

BULK INSERT dbo.man_rle_ruleType 
   FROM 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_rle_ruleType.txt' 
   WITH (FORMATFILE = 'C:\Users\MyUser\Documents\Projects\MantaMTA\Database\Datasets\MANTA_MTA\man_rle_ruleType.xml');
GO