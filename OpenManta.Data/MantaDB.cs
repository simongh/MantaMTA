using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

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
		private readonly IDataRetrieval _dataRetrieval;

		public MantaDB()
		{
			_dataRetrieval = new DataRetrieval();
		}

		/// <summary>
		/// Gets a SqlConnection to the MantaMTA Database
		/// </summary>
		/// <returns>Sql connection</returns>
		public SqlConnection GetSqlConnection()
		{
			return new SqlConnection(ConfigurationManager.ConnectionStrings["Manta"].ConnectionString);
		}

		public SqlCommand GetCommand()
		{
			var cmd = new SqlCommand();
			cmd.Connection = GetSqlConnection();

			return cmd;
		}

		public T GetSingleObjectFromDatabase<T>(string sql, CreateObjectMethod<T> createObjectMethod, Action<SqlCommand> parameters = null)
		{
			using (var cmd = GetCommand())
			{
				cmd.CommandText = sql;
				parameters?.Invoke(cmd);

				return _dataRetrieval.GetSingleObjectFromDatabase(cmd, createObjectMethod);
			}
		}

		public Task<T> GetSingleObjectFromDatabaseAsync<T>(string sql, CreateObjectMethod<T> createObjectMethod, Action<SqlCommand> parameters = null)
		{
			using (var cmd = GetCommand())
			{
				cmd.CommandText = sql;
				parameters?.Invoke(cmd);

				return _dataRetrieval.GetSingleObjectFromDatabaseAsync(cmd, createObjectMethod);
			}
		}

		public bool FillSingleObjectFromDatabase<T>(string sql, T obj, FillObjectMethod<T> fillObjectMethod, Action<SqlCommand> parameters = null)
		{
			using (var cmd = GetCommand())
			{
				cmd.CommandText = sql;
				parameters?.Invoke(cmd);

				return _dataRetrieval.FillSingleObjectFromDatabase(cmd, obj, fillObjectMethod);
			}
		}

		public IEnumerable<T> GetCollectionFromDatabase<T>(string sql, CreateObjectMethod<T> createObjectMethod, Action<SqlCommand> parameters = null)
		{
			using (var cmd = GetCommand())
			{
				cmd.CommandText = sql;
				parameters?.Invoke(cmd);

				return _dataRetrieval.GetCollectionFromDatabase(cmd, createObjectMethod);
			}
		}
	}
}