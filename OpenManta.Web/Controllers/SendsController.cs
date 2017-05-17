﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using WebInterface.Models;
using OpenManta.WebLib.BO;
using OpenManta.WebLib.DAL;
using OpenManta.Core;
using OpenManta.Framework;
using System.Linq;

namespace WebInterface.Controllers
{
	public class SendsController : Controller
	{
		private readonly ISendDB _sendDb;
		private readonly ITransactionDB _transactionDb;
		private readonly IVirtualMtaDB _virtualMtaDb;
		private readonly OpenManta.Data.IMantaDB _mantaDb;

		public SendsController(ISendDB sendDb, ITransactionDB transactionDb, IVirtualMtaDB virtualMtaDb, OpenManta.Data.IMantaDB mantaDb)
		{
			Guard.NotNull(sendDb, nameof(sendDb));
			Guard.NotNull(transactionDb, nameof(transactionDb));
			Guard.NotNull(virtualMtaDb, nameof(virtualMtaDb));
			Guard.NotNull(mantaDb, nameof(mantaDb));

			_sendDb = sendDb;
			_transactionDb = transactionDb;
			_virtualMtaDb = virtualMtaDb;
			_mantaDb = mantaDb;
		}

		//
		// GET: /Sends/
		public ActionResult Index(int page = 1, int pageSize = 10)
		{
			SendInfoCollection sends = _sendDb.GetSends(pageSize, page);
			int pages = (int)Math.Ceiling(_sendDb.GetSendsCount() / Convert.ToDouble(pageSize));
			return View(new SendsModel(sends, page, pages));
		}

		//
		// GET: /Queue/
		public ActionResult Queue()
		{
			SendInfoCollection sends = _sendDb.GetSendsInProgress();
			return View(new SendsModel(sends, 1, 1));
		}

		//
		// GET: /Sends/Overview?sendID=
		public ActionResult Overview(string sendID)
		{
			SendInfo send = _sendDb.GetSend(sendID);
			return View(new SendReportOverview(send));
		}

		//
		// GET: /Sends/VirtualMTA?sendID=
		public ActionResult VirtualMTA(string sendID)
		{
			return View(new SendReportVirtualMta(_virtualMtaDb.GetSendVirtualMTAStats(sendID).ToArray(), sendID));
		}

		//
		// GET: /Sends/Bounces?sendID=
		public ActionResult Bounces(string sendID, int page = 1, int pageSize = 25)
		{
			int bounceCount = _transactionDb.GetBounceCount(sendID);
			int pageCount = (int)Math.Ceiling(bounceCount / Convert.ToDouble(pageSize));
			return View(new SendReportBounces(_transactionDb.GetBounceInfo(sendID, page, pageSize).ToArray(), sendID, page, pageCount));
		}

		//
		// GET: /Sends/Deferred?sendID=
		public ActionResult Deferred(string sendID, int page = 1, int pageSize = 25)
		{
			int bounceCount = _transactionDb.GetDeferredCount(sendID);
			int pageCount = (int)Math.Ceiling(bounceCount / Convert.ToDouble(pageSize));
			return View(new SendReportBounces(_transactionDb.GetDeferralInfo(sendID, page, pageSize).ToArray(), sendID, page, pageCount));
		}

		//
		// GET: /Sends/Failed?sendID=
		public ActionResult Failed(string sendID, int page = 1, int pageSize = 25)
		{
			int bounceCount = _transactionDb.GetFailedCount(sendID);
			int pageCount = (int)Math.Ceiling(bounceCount / Convert.ToDouble(pageSize));
			return View(new SendReportBounces(_transactionDb.GetFailedInfo(sendID, page, pageSize).ToArray(), sendID, page, pageCount));
		}

		//
		// GET: /Sends/Speed?sendID=
		public ActionResult Speed(string sendID)
		{
			return View(new SendReportSpeed(_transactionDb.GetSendSpeedInfo(sendID), sendID));
		}

		//
		// GET: /Sends/Pause?sendID=
		public ActionResult Pause(string sendID, string redirectURL)
		{
			SendManager.Instance.SetSendStatus(sendID, SendStatus.Paused);
			return View(new SendStatusUpdated(redirectURL));
		}

		//
		// GET: /Sends/Resume?sendID=
		public ActionResult Resume(string sendID, string redirectURL)
		{
			SendManager.Instance.SetSendStatus(sendID, SendStatus.Active);
			return View(new SendStatusUpdated(redirectURL));
		}

		//
		// GET: /Sends/Discard?sendID=
		public ActionResult Discard(string sendID, string redirectURL)
		{
			SendManager.Instance.SetSendStatus(sendID, SendStatus.Discard);
			return View(new SendStatusUpdated(redirectURL));
		}

		//
		// GET: /Sends/GetMessageResultCsv?sendID=
		public ActionResult GetMessageResultCsv(string sendID)
		{
			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = @"
DECLARE @sendInternalID int
SELECT @sendInternalID = MtaSendId
FROM Manta.MtaSend WITH(nolock)
WHERE SendId = @sndID

SELECT *
FROM (
SELECT RecipientTo AS 'RCPT',
	(SELECT MAX(CreatedAt) FROM Manta.Transactions as [tran] with(nolock) WHERE [tran].MessageId = [msg].MessageId) as 'Timestamp',
	(SELECT TOP 1 TransactionStatusId FROM Manta.Transactions as [tran] with(nolock) WHERE [tran].MessageId = [msg].MessageId ORDER BY [tran].CreatedAt DESC) as 'Status',
	(SELECT TOP 1 ServerHostname FROM Manta.Transactions as [tran] with(nolock) WHERE [tran].MessageId = [msg].MessageId ORDER BY [tran].CreatedAt DESC) as 'Remote',
	(SELECT TOP 1 ServerResponse FROM Manta.Transactions as [tran] with(nolock) WHERE [tran].MessageId = [msg].MessageId ORDER BY [tran].CreatedAt DESC) as 'Response'
FROM Manta.Messages as [msg] with(nolock)
WHERE [msg].MtaSendId = @sendInternalID ) as [ExportData]
ORDER BY [ExportData].Timestamp ASC";
				cmd.Parameters.AddWithValue("@sndID", sendID);
				DataTable dt = new DataTable();
				conn.Open();
				SqlDataAdapter da = new SqlDataAdapter(cmd);
				da.Fill(dt);
				return View(dt);
			}
		}
	}
}