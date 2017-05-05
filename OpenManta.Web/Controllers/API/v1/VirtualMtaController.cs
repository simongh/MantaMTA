using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using OpenManta.Core;
using OpenManta.Data;
using OpenManta.WebLib;
using WebInterface.Models.API.v1;

namespace WebInterface.Controllers.API.v1
{
	/// <summary>
	/// Summary description for VirtualMta API
	/// </summary>
	[RoutePrefix("api/v1/VirtualMta")]
	public class VirtualMtaController : ApiController
	{
		private readonly IVirtualMtaWebManager _manager;
		private readonly OpenManta.WebLib.DAL.IVirtualMtaDB _virtualMtaDb;

		public VirtualMtaController(IVirtualMtaWebManager manager, OpenManta.WebLib.DAL.IVirtualMtaDB virtualMtaDb)
		{
			Guard.NotNull(manager, nameof(manager));
			Guard.NotNull(virtualMtaDb, nameof(virtualMtaDb));

			_manager = manager;
			_virtualMtaDb = virtualMtaDb;
		}

		/// <summary>
		/// Updates an existing Virtual MTA.
		/// </summary>
		/// <param name="viewModel"></param>
		/// <returns>TRUE if updated or FALSE if update failed.</returns>
		[HttpPost]
		[Route("Save")]
		public bool Save(SaveVirtualMtaViewModel viewModel)
		{
			VirtualMTA vMTA = null;

			if (viewModel.Id != WebInterfaceParameters.VIRTUALMTA_NEW_ID)
				vMTA = VirtualMtaDB.GetVirtualMta(viewModel.Id);
			else
				vMTA = new VirtualMTA();

			if (vMTA == null)
				return false;

			if (string.IsNullOrWhiteSpace(viewModel.HostName))
				return false;

			IPAddress ip = null;
			try
			{
				ip = IPAddress.Parse(viewModel.IpAddress);
			}
			catch (Exception)
			{
				return false;
			}

			vMTA.Hostname = viewModel.HostName;
			vMTA.IPAddress = ip;
			vMTA.IsSmtpInbound = viewModel.Inbound;
			vMTA.IsSmtpOutbound = viewModel.Outbound;
			_virtualMtaDb.Save(vMTA);
			return true;
		}

		/// <summary>
		/// Deletes the specified Virtual MTA.
		/// </summary>
		/// <param name="viewModel"></param>
		[HttpPost]
		[Route("Delete")]
		public void Delete(DeleteVirtualMtaViewModel viewModel)
		{
			_virtualMtaDb.Delete(viewModel.Id);
		}

		/// <summary>
		/// Saves the Virtual MTA Group.
		/// </summary>
		/// <param name="viewModel"></param>
		/// <returns>TRUE if saved or FALSE if not saved.</returns>
		[HttpPost]
		[Route("SaveGroup")]
		public bool SaveGroup(SaveVirtualMtaGroupViewModel viewModel)
		{
			VirtualMtaGroup grp = null;
			if (viewModel.Id == WebInterfaceParameters.VIRTUALMTAGROUP_NEW_ID)
				grp = new VirtualMtaGroup();
			else
				grp = VirtualMtaGroupDB.GetVirtualMtaGroup(viewModel.Id);

			if (grp == null)
				return false;

			grp.Name = viewModel.Name;
			grp.Description = viewModel.Description;

			var vMtas = VirtualMtaDB.GetVirtualMtas();
			for (int i = 0; i < viewModel.MtaIDs.Length; i++)
			{
				VirtualMTA mta = vMtas.SingleOrDefault(m => m.ID == viewModel.MtaIDs[i]);
				if (mta == null)
					return false;
				grp.VirtualMtaCollection.Add(mta);
			}

			_manager.Save(grp);
			return true;
		}

		/// <summary>
		/// Deletes a Virtual MTA Group.
		/// </summary>
		/// <param name="viewModel">ID of the Virtual MTA Group to delete.</param>
		[HttpPost]
		[Route("DeleteGroup")]
		public void DeleteGroup(DeleteVirtualMtaGroupViewModel viewModel)
		{
			_manager.DeleteGroup(viewModel.Id);
		}
	}
}