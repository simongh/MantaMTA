using System.Collections.Generic;
using OpenManta.Core;

namespace OpenManta.Framework
{
	public interface IVirtualMtaManager
	{
		IList<VirtualMTA> GetVirtualMtasForListeningOn();

		IList<VirtualMTA> GetVirtualMtasForSending();

		VirtualMtaGroup GetDefaultVirtualMtaGroup();

		VirtualMtaGroup GetVirtualMtaGroup(int id);
	}
}