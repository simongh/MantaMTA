using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace OpenManta.Data
{
	public interface IMantaDB
	{
		SqlConnection GetSqlConnection();

		T GetSingleObjectFromDatabase<T>(string sql, CreateObjectMethod<T> createObjectMethod, Action<SqlCommand> parameters = null);

		Task<T> GetSingleObjectFromDatabaseAsync<T>(string sql, CreateObjectMethod<T> createObjectMethod, Action<SqlCommand> parameters = null);

		bool FillSingleObjectFromDatabase<T>(string sql, T obj, FillObjectMethod<T> fillObjectMethod, Action<SqlCommand> parameters = null);

		IEnumerable<T> GetCollectionFromDatabase<T>(string sql, CreateObjectMethod<T> createObjectMethod, Action<SqlCommand> parameters = null);
	}
}