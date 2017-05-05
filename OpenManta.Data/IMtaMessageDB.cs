using System;
using System.Threading.Tasks;

namespace OpenManta.Data
{
	public interface IMtaMessageDB
	{
		Task<string> GetMailFrom(Guid messageId);
	}
}