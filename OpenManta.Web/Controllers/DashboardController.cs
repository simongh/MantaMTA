using OpenManta.Core;
using OpenManta.Framework;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using WebInterface.Models;
using OpenManta.WebLib.DAL;

namespace WebInterface.Controllers
{
	public class DashboardController : Controller
	{
		private readonly ISendDB _sendDb;
		private readonly ITransactionDB _transactionDb;

		public DashboardController(ISendDB sendDb, ITransactionDB transactionDb)
		{
			Guard.NotNull(sendDb, nameof(sendDb));
			Guard.NotNull(transactionDb, nameof(transactionDb));

			_sendDb = sendDb;
			_transactionDb = transactionDb;
		}

		//
		// GET: /Dashboard/
		public ActionResult Index()
		{
			DashboardModel model = new DashboardModel
			{
				SendTransactionSummaryCollection = _transactionDb.GetLastHourTransactionSummary(),
				Waiting = _sendDb.GetQueueCount(new SendStatus[] { SendStatus.Active, SendStatus.Discard }),
				Paused = _sendDb.GetQueueCount(new SendStatus[] { SendStatus.Paused }),
				BounceInfo = _transactionDb.GetLastHourBounceInfo(3).ToArray(),
				SendSpeedInfo = _transactionDb.GetLastHourSendSpeedInfo()
			};

			try
			{
				// Connect to Rabbit MQ and grab basic queue counts.
				HttpWebRequest request = HttpWebRequest.CreateHttp("http://localhost:15672/api/queues");
				request.Credentials = new NetworkCredential(MtaParameters.RabbitMQ.Username, MtaParameters.RabbitMQ.Password);
				using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
				{
					string json = new StreamReader(response.GetResponseStream()).ReadToEnd();
					JArray rabbitQueues = JArray.Parse(json);
					foreach (JToken q in rabbitQueues.Children())
					{
						JEnumerable<JProperty> qProperties = q.Children<JProperty>();
						string queueName = (string)qProperties.First(x => x.Name.Equals("name")).Value;
						if (queueName.StartsWith("manta_mta_"))
						{
							long messages = (long)qProperties.First(x => x.Name.Equals("messages", System.StringComparison.OrdinalIgnoreCase)).Value;
							if (queueName.IndexOf("_inbound") > 0)
								model.RabbitMqInbound += messages;
							else if (queueName.IndexOf("_outbound_") > 0)
								model.RabbitMqTotalOutbound += messages;
						}
					}
				}
			}
			catch (Exception)
			{
				model.RabbitMqInbound = int.MinValue;
				model.RabbitMqTotalOutbound = int.MinValue;
			}

			return View(model);
		}
	}
}