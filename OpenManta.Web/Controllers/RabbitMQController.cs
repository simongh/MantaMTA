using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using OpenManta.Core;
using OpenManta.Framework;
using WebInterface.Models;

namespace WebInterface.Controllers
{
	public class RabbitMQController : Controller
	{
		private readonly IMtaParameters _config;

		public RabbitMQController(IMtaParameters config)
		{
			Guard.NotNull(config, nameof(config));

			_config = config;
		}

		//
		// GET: /RabbitMQ/
		public ActionResult Index()
		{
			RabbitMqQueueModel model = new RabbitMqQueueModel();
			// Connect to Rabbit MQ and grab basic queue counts.
			HttpWebRequest request = HttpWebRequest.CreateHttp("http://localhost:15672/api/queues");
			request.Credentials = new NetworkCredential(_config.RabbitMq.Username, _config.RabbitMq.Password);
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
						model.Add(new RabbitMqQueue
						{
							Name = queueName,
							Messages = (long)qProperties.First(x => x.Name.Equals("messages")).Value,
							State = (string)qProperties.First(x => x.Name.Equals("state")).Value
						});
					}
				}
			}
			return View(model);
		}
	}
}