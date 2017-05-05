using System.Collections.Generic;
using OpenManta.Core;

namespace OpenManta.Data
{
	public interface IVirtualMtaDB
	{
		IList<VirtualMTA> GetVirtualMtas();

		VirtualMTA GetVirtualMta(int id);

		IList<VirtualMTA> GetVirtualMtasInVirtualMtaGroup(int groupID);
	}
}