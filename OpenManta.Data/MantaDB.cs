using System.Configuration;
using System.Data.SqlClient;

namespace OpenManta.Data
{
	public static class MantaDbFactory
	{
		public static IMantaDB Instance { get; internal set; }
	}

	/// <summary>
	/// Functions to help with database stuff.
	/// </summary>
	internal class MantaDB : IMantaDB
	{
		/// <summary>
		/// Gets a SqlConnection to the MantaMTA Database
		/// </summary>
		/// <returns>Sql connection</returns>
		public SqlConnection GetSqlConnection()
		{
			return new SqlConnection(ConfigurationManager.ConnectionStrings["SqlServer"].ConnectionString);
		}
	}
}