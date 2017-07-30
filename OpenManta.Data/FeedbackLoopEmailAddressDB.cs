using System.Data.SqlClient;
using OpenManta.Core;

namespace OpenManta.Data
{
	//public static class FeedbackLoopEmailAddressDBFactory
	//{
	//	public static IFeedbackLoopEmailAddressDB Instance { get; internal set; }
	//}

	internal class FeedbackLoopEmailAddressDB : IFeedbackLoopEmailAddressDB
	{
		private readonly IMantaDB _mantaDb;

		public FeedbackLoopEmailAddressDB(IMantaDB mantaDb)
		{
			Guard.NotNull(mantaDb, nameof(mantaDb));

			_mantaDb = mantaDb;
		}

		/// <summary>
		/// Checks an address to see if it appears in the list of feedback loop addresses.
		/// </summary>
		/// <param name="address">Address to check.</param>
		/// <returns>TRUE if exists, FALSE if not.</returns>
		public bool IsFeedbackLoopEmailAddress(string address)
		{
			using (SqlConnection conn = _mantaDb.GetSqlConnection())
			{
				SqlCommand cmd = conn.CreateCommand();
				cmd.CommandText = @"
SELECT 1
FROM Manta.FeedbackLoopAddresses
WHERE Address = @address";
				cmd.Parameters.AddWithValue("@address", address);
				conn.Open();
				object result = cmd.ExecuteScalar();
				if (result == null)
					return false;

				return true;
			}
		}
	}
}