using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OpenManta.Core;
using OpenManta.Framework;
using OpenManta.WebLib;
using OpenManta.WebLib.DAL;
using OpenManta.WebLib.BO;

namespace WebInterface
{
	/// <summary>
	/// Summary description for AppendSendMetadata
	/// </summary>
	public class AppendSendMetadata : IHttpHandler
	{
		private readonly ISendDB _sendDb;

		public AppendSendMetadata(ISendDB sendDb)
		{
			Guard.NotNull(sendDb, nameof(sendDb));

			_sendDb = sendDb;
		}

		public void ProcessRequest(HttpContext context)
		{
			context.Response.ContentType = "text/plain";

			string[] relayingIPs = MtaParameters.IPsToAllowRelaying;
			if (!relayingIPs.Contains(context.Request.UserHostAddress))
			{
				context.Response.Write("Forbidden");
				return;
			}

			string sendID = context.Request.QueryString["SendID"];
			string name = context.Request.QueryString["Name"];
			string value = context.Request.QueryString["Value"];

			if (string.IsNullOrWhiteSpace(sendID) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(value))
			{
				context.Response.Write("bad");
				return;
			}

			Send snd = SendManager.Instance.GetSend(sendID);
			_sendDb.SaveSendMetadata(snd.InternalID, new SendMetadata
			{
				Name = name,
				Value = value
			});

			context.Response.Write("ok");
		}

		public bool IsReusable
		{
			get
			{
				return true;
			}
		}
	}
}