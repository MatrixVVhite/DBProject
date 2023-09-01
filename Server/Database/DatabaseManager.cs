using MySql.Data.MySqlClient;
using Server.DataStructures;
using System.Data;
using System.Diagnostics;

namespace Server.Database
{
	public sealed class DatabaseManager
	{
		#region SQL_FIELDS
		private const string CON_STR = "Server=localhost; database=finalprojectdb; UID=root; password=rootPass";
		private MySqlConnection _conn;
		private MySqlDataAdapter _data;
		#endregion

		#region SINGLETON_PROPERTIES
		private static readonly Lazy<DatabaseManager> _lazy = new Lazy<DatabaseManager>(() => new DatabaseManager());

		public static DatabaseManager Instance { get => _lazy.Value; }
		#endregion

		#region CONSTRUCTORS
		public DatabaseManager()
		{
			DefineConnection();
		}
		#endregion

		#region CONNECTION
		private bool TryConnect()
		{
			bool success;
			try
			{
				OpenConnection();
				success = true;
			}
			catch (MySqlException e)
			{
				Debug.Write(e);
				CloseConnection();
				success = false;
			}
			return success;
		}

		private void DefineConnection()
		{
			_conn = new MySqlConnection(CON_STR);
		}

		private void OpenConnection()
		{
			_conn.Open();
		}

		private void CloseConnection()
		{
			_conn.Close();
		}
		#endregion

		#region QUERIES
		#region SELECT
		private DataTable SelectQuery(string query)
		{
			Debug.Assert(query.ToLower().Contains("select"));
			_data = new MySqlDataAdapter(query, _conn);
			_data.SelectCommand.CommandType = CommandType.Text;
			var dt = new DataTable();
			try
			{
				_data.Fill(dt);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}

			return dt;
		}

		public Question GetQuestion(int id)
		{
			string query = $"SELECT * FROM questions WHERE QuestionID = {id};";

			try
			{
				return new(SelectQuery(query).Rows[0]);
			}
			catch (IndexOutOfRangeException ex)
			{
				Debug.WriteLine(ex);
				return new Question();
			}
		}
		#endregion
		#endregion
	}
}
