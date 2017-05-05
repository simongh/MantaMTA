using System;
using System.Linq;
using System.Web.Mvc;
using OpenManta.Core;
using OpenManta.WebLib.DAL;
using WebInterface.Models;

namespace WebInterface.Controllers
{
	public class BouncesController : Controller
	{
		private readonly ITransactionDB _transactionDb;

		public BouncesController(ITransactionDB transactionDb)
		{
			Guard.NotNull(transactionDb, nameof(transactionDb));

			_transactionDb = transactionDb;
		}

		//
		// GET: /Bounces/
		public ActionResult Index(int? page = 1, int? pageSize = 25)
		{
			long deferred, rejected = 0;
			_transactionDb.GetBounceDeferredAndRejected(out deferred, out rejected);
			return View(new BounceModel
			{
				BounceInfo = _transactionDb.GetBounceInfo(null, page.Value, pageSize.Value).ToArray(),
				CurrentPage = page.Value,
				PageCount = (int)Math.Ceiling(Convert.ToDouble(_transactionDb.GetBounceCount(null)) / pageSize.Value),
				DeferredCount = deferred,
				RejectedCount = rejected
			});
		}
	}
}