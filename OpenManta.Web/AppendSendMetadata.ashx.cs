using System.Linq;
using System.Web;
using Ninject;
using OpenManta.Core;
using OpenManta.Framework;
using OpenManta.WebLib.BO;
using OpenManta.WebLib.DAL;

namespace WebInterface
{
	internal class AppendSendMetadataHelper
	{
		private static IKernel _kernel;

		public ISendDB SendDb { get; private set; }
		public IMtaParameters Config { get; private set; }

		internal static void Initialise(IKernel kernel)
		{
			Guard.NotNull(kernel, nameof(kernel));

			_kernel = kernel;
		}

		public static AppendSendMetadataHelper Create()
		{
			return _kernel.Get<AppendSendMetadataHelper>();
		}

		public AppendSendMetadataHelper(ISendDB sendDb, IMtaParameters config)
		{
			Guard.NotNull(sendDb, nameof(sendDb));
			Guard.NotNull(config, nameof(config));

			SendDb = sendDb;
			Config = config;
		}
	}

	/// <summary>
	/// Summary description for AppendSendMetadata
	/// </summary>
	public class AppendSendMetadata : IHttpHandler
	{
		private readonly AppendSendMetadataHelper _helper;

		public AppendSendMetadata()
		{
			_helper = AppendSendMetadataHelper.Create();
		}

		public void ProcessRequest(HttpContext context)
		{
			context.Response.ContentType = "text/plain";

			var relayingIPs = _helper.Config.IPsToAllowRelaying;
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
			_helper.SendDb.SaveSendMetadata(snd.InternalID, new SendMetadata
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