using System.Data.SqlClient;

namespace OpenManta.Data
{
	public interface IMantaDB
	{
		SqlConnection GetSqlConnection();
	}
}