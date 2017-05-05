using System.Collections.Generic;
using OpenManta.Core;

namespace OpenManta.WebLib
{
	public interface IVirtualMtaWebManager
	{
		IList<VirtualMtaGroup> GetAllVirtualMtaGroups();

		VirtualMtaGroup GetVirtualMtaGroup(int id);

		void Save(VirtualMtaGroup grp);

		void DeleteGroup(int id);
	}
}