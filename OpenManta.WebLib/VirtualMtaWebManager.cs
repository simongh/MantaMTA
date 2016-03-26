using System.Collections.Generic;
using OpenManta.Data;
using OpenManta.Core;

namespace OpenManta.WebLib
{
	public static class VirtualMtaWebManager
	{
		/// <summary>
		/// Get a collection of all of the Virtual MTA Groups.
		/// </summary>
		/// <returns></returns>
		public static IList<VirtualMtaGroup> GetAllVirtualMtaGroups()
		{
			IList<VirtualMtaGroup> ipGroups = VirtualMtaGroupDB.GetVirtualMtaGroups();
			
			// Get all the groups Virtual MTAs.
			foreach (VirtualMtaGroup grp in ipGroups)
			{
				grp.VirtualMtaCollection = VirtualMtaDB.GetVirtualMtasInVirtualMtaGroup(grp.ID);
			}

			return ipGroups;
		}

		/// <summary>
		/// Gets a single Virtual MTA Group.
		/// </summary>
		/// <param name="id">ID of the Virtual MTA Group to get.</param>
		/// <returns>The Virtual MTA Group or NULL if none exist with ID</returns>
		public static VirtualMtaGroup GetVirtualMtaGroup(int id)
		{
			VirtualMtaGroup grp = VirtualMtaGroupDB.GetVirtualMtaGroup(id);
			grp.VirtualMtaCollection = VirtualMtaDB.GetVirtualMtasInVirtualMtaGroup(grp.ID);
			return grp;
		}

		/// <summary>
		/// Saves the Virtual MTA Group.
		/// </summary>
		/// <param name="grp">Virtual MTA Group to save.</param>
		public static void Save(VirtualMtaGroup grp)
		{
			VirtualMtaGroupDB.Save(grp);
		}

		/// <summary>
		/// Deletes a Virtual MTA Group.
		/// </summary>
		/// <param name="id">ID of the group to delete.</param>
		public static void DeleteGroup(int id)
		{
			VirtualMtaGroupDB.Delete(id);
		}
	}
}
