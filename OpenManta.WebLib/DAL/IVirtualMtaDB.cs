using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenManta.Core;
using OpenManta.WebLib.BO;

namespace OpenManta.WebLib.DAL
{
	public interface IVirtualMtaDB
	{
		IEnumerable<VirtualMtaSendInfo> GetSendVirtualMTAStats(string sendID);

		VirtualMtaSendInfo CreateAndFillVirtualMtaSendInfo(IDataRecord record);

		void Save(VirtualMTA vmta);

		void Delete(int id);
	}
}