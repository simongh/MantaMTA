using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace OpenManta.Data
{
	public interface IDataRetrieval
	{
		ObjectType GetSingleObjectFromDatabase<ObjectType>(SqlCommand command, CreateObjectMethod<ObjectType> createObjectMethod);

		Task<T> GetSingleObjectFromDatabaseAsync<T>(SqlCommand command, CreateObjectMethod<T> createObjectMethod);

		bool FillSingleObjectFromDatabase<ObjectType>(SqlCommand command, ObjectType obj, FillObjectMethod<ObjectType> fillObjectMethod);

		List<ObjectType> GetCollectionFromDatabase<ObjectType>(SqlCommand command, CreateObjectMethod<ObjectType> createObjectMethod);
	}
}