using MySql.Data;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using System;
using System.Diagnostics;

namespace Server.Database
{
	public sealed class DatabaseManager
	{
		#region SQL_FIELDS
		private const string CON_STR = "Server=localhost; database=finalprojectdb; UID=root; password=rootPass";
		private MySqlConnection _conn;
		private MySqlCommand _cmd;
		private MySqlDataReader _reader;
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
		public string? GetQuestionText(int id)
		{
			TryConnect();
			string? result;
			string query = $"SELECT QuestionText FROM questions WHERE QuestionID = {id};";
			_cmd = new MySqlCommand(query, _conn);
			_reader = _cmd.ExecuteReader();

			if (_reader.Read())
				result = _reader.GetString(0);
			else
				result = null;
			CloseConnection();

			return result;
		}
		#endregion
	}
}
