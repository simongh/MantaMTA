using OpenManta.Core;
using OpenManta.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using log4net;

namespace OpenManta.Framework
{
	internal class QueueManager : IQueueManager, IStopRequired
	{
		/// <summary>
		/// The maximum time between looking for messages that have been queued in RabbitMQ.
		/// </summary>
		private readonly TimeSpan RABBITMQ_EMPTY_QUEUE_SLEEP_TIME = TimeSpan.FromSeconds(10);

		/// <summary>
		/// Will be set to true when the Stop() method is called.
		/// </summary>
		private volatile bool _isStopping = false;

		private readonly ILog _logging;
		private readonly IMantaDB _mantaDb;

		private QueueManager(IMantaCoreEvents coreEvents, log4net.ILog logging, IMantaDB mantaDb)
		{
			Guard.NotNull(coreEvents, nameof(coreEvents));
			Guard.NotNull(logging, nameof(logging));
			Guard.NotNull(mantaDb, nameof(mantaDb));

			_logging = logging;
			_mantaDb = mantaDb;

			coreEvents.RegisterStopRequiredInstance(this);
		}

		/// <summary>
		/// Enqueues the Inbound Message for Relaying.
		/// </summary>
		/// <param name="messageID">ID of the Message being Queued.</param>
		/// <param name="ipGroupID">ID of the Virtual MTA Group to send the Message through.</param>
		/// <param name="internalSendID">ID of the Send the Message is apart of.</param>
		/// <param name="mailFrom">The envelope mailfrom, should be return-path in most instances.</param>
		/// <param name="rcptTo">The envelope rcpt to.</param>
		/// <param name="message">The Email.</param>
		/// <param name="priority">Priority of message.</param>
		/// <returns>True if the Message has been queued, false if not.</returns>
		public async Task<bool> Enqueue(Guid messageID, int ipGroupID, int internalSendID, string mailFrom, string[] rcptTo, string message, RabbitMqPriority priority)
		{
			return await RabbitMq.RabbitMqInboundQueueManager.Enqueue(messageID, ipGroupID, internalSendID, mailFrom, rcptTo, message, priority);
		}

		/// <summary>
		/// Start the bulk importer.
		/// </summary>
		public void Start()
		{
			Thread t = new Thread(new ThreadStart(DoSqlBulkInsertFromRabbitMQ));
			t.Start();
		}

		/// <summary>
		/// Stop the bulk importer.
		/// </summary>
		public void Stop()
		{
			_isStopping = true;
		}

		/// <summary>
		/// Does the actual bulk importing from RabbitMQ to SQL Server.
		/// </summary>
		private void DoSqlBulkInsertFromRabbitMQ()
		{
			// Keep going until Manta is stopping.
			while (!_isStopping)
			{
				try
				{
					// Get queued messages for bulk importing.
					IList<MtaMessage> recordsToImportToSql = RabbitMq.RabbitMqInboundQueueManager.Dequeue(100).Result;

					// If there are no messages to import then sleep and try again.
					if (recordsToImportToSql == null || recordsToImportToSql.Count == 0)
					{
						var sleepCount = RABBITMQ_EMPTY_QUEUE_SLEEP_TIME.TotalSeconds / 2;
						for (int i = 0; i < sleepCount; i++)
						{
							Thread.Sleep(2000);
							if (_isStopping)
								break;
						}
						continue;
					}

					DataTable dt = new DataTable();
					dt.Columns.Add("mta_msg_id", typeof(Guid));
					dt.Columns.Add("mta_send_internalId", typeof(int));
					dt.Columns.Add("mta_msg_rcptTo", typeof(string));
					dt.Columns.Add("mta_msg_mailFrom", typeof(string));

					foreach (MtaMessage msg in recordsToImportToSql)
						dt.Rows.Add(new object[] { msg.ID, msg.InternalSendID, msg.RcptTo[0], msg.MailFrom });

					try
					{
						// Create a record of the messages in SQL server.
						using (SqlConnection conn = _mantaDb.GetSqlConnection())
						{
							SqlBulkCopy bulk = new SqlBulkCopy(conn);
							bulk.DestinationTableName = "man_mta_msg_staging";
							foreach (DataColumn c in dt.Columns)
								bulk.ColumnMappings.Add(c.ColumnName, c.ColumnName);

							conn.Open();
							bulk.WriteToServer(dt);
							SqlCommand cmd = conn.CreateCommand();
							cmd.CommandText = @"
BEGIN TRANSACTION
MERGE man_mta_msg AS target
    USING (SELECT * FROM man_mta_msg_staging) AS source
    ON (target.[mta_msg_id] = source.[mta_msg_id])
	WHEN NOT MATCHED THEN
		INSERT ([mta_msg_id], [mta_send_internalId], [mta_msg_rcptTo], [mta_msg_mailFrom])
		VALUES (source.[mta_msg_id], source.[mta_send_internalId], source.[mta_msg_rcptTo],  source.[mta_msg_mailFrom]);

DELETE FROM [man_mta_msg_staging]
COMMIT TRANSACTION";
							cmd.ExecuteNonQuery();
						}

						RabbitMq.RabbitMqManager.Ack(RabbitMq.RabbitMqManager.RabbitMqQueue.Inbound, recordsToImportToSql.Max(r => r.RabbitMqDeliveryTag), true);
					}
					catch (Exception ex)
					{
						_logging.Warn("Server Queue Manager", ex);
					}
				}
				catch (Exception)
				{
					//Logging.Error("Bulk Importer Error", ex);
				}
			}
		}
	}
}