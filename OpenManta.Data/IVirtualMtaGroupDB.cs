using System.Collections.Generic;
using OpenManta.Core;

namespace OpenManta.Data
{
	public interface IVirtualMtaGroupDB
	{
		VirtualMtaGroup GetVirtualMtaGroup(int id);

		IList<VirtualMtaGroup> GetVirtualMtaGroups();

		void Save(VirtualMtaGroup grp);

		void Delete(int id);
	}
}