using System.Web.Mvc;
using OpenManta.Core;
using OpenManta.Framework;
using WebInterface.Models;

namespace WebInterface.Controllers
{
	public class ServerStatusController : Controller
	{
		private readonly IMtaParameters _config;

		public ServerStatusController(IMtaParameters config)
		{
			Guard.NotNull(config, nameof(config));

			_config = config;
		}

		//
		// GET: /ServerStatus/
		public ActionResult Index()
		{
			var result = new ServerStatusModel();

			result.QueueDir = new ServerStatusDirectoryInfo(_config.MTA_QUEUEFOLDER);
			result.LogDir = new ServerStatusDirectoryInfo(_config.MTA_SMTP_LOGFOLDER);
			result.DropDir = new ServerStatusDirectoryInfo(_config.MTA_DROPFOLDER);

			return View(result);
		}
	}
}