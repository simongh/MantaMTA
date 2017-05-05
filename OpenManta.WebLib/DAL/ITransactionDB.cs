using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenManta.WebLib.BO;

namespace OpenManta.WebLib.DAL
{
	public interface ITransactionDB
	{
		SendSpeedInfo GetSendSpeedInfo(string sendID);

		SendSpeedInfo GetLastHourSendSpeedInfo();

		IEnumerable<BounceInfo> GetBounceInfo(string sendID, int pageNum, int pageSize);

		IEnumerable<BounceInfo> GetFailedInfo(string sendID, int pageNum, int pageSize);

		IEnumerable<BounceInfo> GetDeferralInfo(string sendID, int pageNum, int pageSize);

		IEnumerable<BounceInfo> GetLastHourBounceInfo(int count);

		int GetBounceCount(string sendID);

		int GetDeferredCount(string sendID);

		int GetFailedCount(string sendID);

		void GetBounceDeferredAndRejected(out long deferred, out long rejected);

		SendTransactionSummaryCollection GetLastHourTransactionSummary();
	}
}