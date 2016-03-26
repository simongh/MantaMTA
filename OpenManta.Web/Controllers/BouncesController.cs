using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebInterface.Models;
using OpenManta.WebLib.DAL;

namespace WebInterface.Controllers
{
    public class BouncesController : Controller
    {
        //
        // GET: /Bounces/
        public ActionResult Index(int? page = 1, int? pageSize = 25)
        {
			long deferred, rejected = 0;
			TransactionDB.GetBounceDeferredAndRejected(out deferred, out rejected);
			return View(new BounceModel
			{
				BounceInfo =TransactionDB.GetBounceInfo(null, page.Value, pageSize.Value),
				CurrentPage = page.Value,
				PageCount = (int)Math.Ceiling(Convert.ToDouble(TransactionDB.GetBounceCount(null)) / pageSize.Value),
				DeferredCount = deferred,
				RejectedCount = rejected
			});
        }

    }
}
