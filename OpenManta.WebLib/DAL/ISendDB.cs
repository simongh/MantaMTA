using OpenManta.Core;
using OpenManta.WebLib.BO;

namespace OpenManta.WebLib.DAL
{
	public interface ISendDB
	{
		long GetQueueCount(SendStatus[] sendStatus);

		long GetSendsCount();

		SendInfoCollection GetSends(int pageSize, int pageNum);

		SendInfoCollection GetSendsInProgress();

		SendInfo GetSend(string sendID);

		SendMetadataCollection GetSendMetaData(int internalSendID);

		bool SaveSendMetadata(int internalSendID, SendMetadata metadata);
	}
}