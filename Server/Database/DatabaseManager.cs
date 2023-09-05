using MySql.Data.MySqlClient;
using System.Data;
using System.Diagnostics;
using JsonDict = System.Collections.Generic.Dictionary<string, object?>;

namespace Server.Database
{
	public sealed class DatabaseManager
	{
		#region FIELDS
		#region SQL_FIELDS
		private const string CON_STR = "Server=localhost; database=finalprojectdb; UID=root; password=rootPass";
		private MySqlConnection _conn;
		private MySqlDataAdapter _data;
		#endregion

		#region SINGLETON_PROPERTIES
		private static readonly Lazy<DatabaseManager> _lazy = new Lazy<DatabaseManager>(() => new DatabaseManager());

		public static DatabaseManager Instance { get => _lazy.Value; }
		#endregion
		#endregion

		#region METHODS
		#region CONSTRUCTORS
		public DatabaseManager()
		{
			DefineConnection();
		}
		#endregion

		#region UTILITY
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

		#region COMMAND
		private int ExecuteCommand(string statement)
		{
			return CreateCommand(statement).ExecuteNonQuery();
		}

		private MySqlCommand CreateCommand(string statement)
		{
			var cmd = _conn.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = statement;
			return cmd;
		}
		#endregion

		#region DATA_TO_DICT
		static private JsonDict DataTableToDictionary(DataTable dataTable)
		{
			JsonDict dict;
			int ROWS_COUNT = dataTable.Rows.Count;
			if (ROWS_COUNT > 1)
			{
				dict = new(ROWS_COUNT);
				int rowNumber = 1;
				foreach (DataRow row in dataTable.Rows)
					dict.Add(rowNumber++.ToString(), DataRowToDictionary(row));
			}
			else
			{
				dict = DataRowToDictionary(dataTable);
			}
			return dict;
		}

		static private JsonDict DataRowToDictionary(DataTable dataTable)
		{
			try
			{
				return DataRowToDictionary(dataTable.Rows[0]);
			}
			catch (IndexOutOfRangeException ex)
			{
				Debug.WriteLine(ex.Message);
				return new JsonDict();
			}
		}

		static private JsonDict DataRowToDictionary(DataRow row)
		{
			var dataTable = row.Table;
			JsonDict dict = new(row.ItemArray.Length);
			foreach (DataColumn dataColumn in dataTable.Columns)
				dict.Add(dataColumn.ColumnName, row[dataColumn.ColumnName]?.ToString());
			return dict;
		}
		#endregion
		#endregion

		#region MYSQL_STATEMENTS
		#region QUERIES
		private JsonDict ExecuteQuery(string query)
		{
			Debug.Assert(query.ToLower().Contains("select"));
			_data = new MySqlDataAdapter(query, _conn);
			_data.SelectCommand.CommandType = CommandType.Text;
			var dt = new DataTable();
			try
			{
				_data.Fill(dt);
			}
			catch (MySqlException ex)
			{
				Debug.WriteLine(ex);
			}

			return DataTableToDictionary(dt);
		}

		public JsonDict GetQuestion(int id)
		{
			string query = $"SELECT * FROM questions WHERE QuestionID = {id};";
			return ExecuteQuery(query);
		}

		/// <summary>
		/// Returns whether this unique token already exists for a player.
		/// </summary>
		/// <param name="playerToken">Unique token to check for</param>
		/// <returns>Whether this unique token already exists</returns>
		public bool GetPlayerTokenExists(int playerToken)
		{
			throw new NotImplementedException(); // TODO Implement
		}

		/// <summary>
		/// Returns whether this player has a ticket in the waiting queue.
		/// </summary>
		/// <param name="playerToken">Unique token of the requesting player</param>
		/// <returns>Whether this player has a ticket</returns>
		public bool GetTicketExists(int playerToken)
		{
			throw new NotImplementedException(); // TODO Implement
		}

		/// <summary>
		/// Returns whether the server has found a match for this player. And if it did - the match ID.
		/// </summary>
		/// <param name="playerToken">Unique token of the requesting player</param>
		/// <returns>Whether the server has found a match, and match ID</returns>
		public (bool, int) GetMatchFound(int playerToken)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns all relevant information about the match's status, such as:
		/// Both players' scores.
		/// How many questions both players have already answered.
		/// How many questions are left.
		/// Notify if the other player disconnected.
		/// </summary>
		/// <param name="matchID">Unique ID of the match to check</param>
		/// <returns>A dictionary with all the relevant info</returns>
		public JsonDict GetMatchStatus(int matchID)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns all the relevant information about this player's next/current questions, such as:
		/// Question's ID.
		/// Question's number in the match.
		/// Question text.
		/// All 4 possible question answers.
		/// </summary>
		/// <param name="playerToken">Unique token of the player to check the question for</param>
		/// <returns>A dictionary with all the relevant info</returns>
		public JsonDict GetNextQuestionForPlayer(int playerToken)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region INSERT/UPDATE/DELETE
		private int ExecuteInsertUpdate(string statement)
		{
			Debug.Assert(statement.ToLower().Contains("insert") || statement.ToLower().Contains("update"));
			int rowsAffected = 0;
			if (TryConnect())
			{
				try
				{
					ExecuteCommand(statement);
				}
				catch (MySqlException ex)
				{
					Debug.WriteLine(ex);
				}
				finally
				{
					CloseConnection();
				}
			}
			return rowsAffected;
		}

		/// <summary>
		/// Connects this player to the server and adds them to the Players table.
		/// </summary>
		/// <param name="playerToken">Registers a unique token for the player to identify with</param>
		/// <param name="playerName">Registers this name as the player's alias</param>
		/// <returns>Success/Failure</returns>
		public bool AddNewPlayer(int playerToken, string playerName)
		{
			// Note: You don't have to check for unique token here
			throw new NotImplementedException(); // TODO Implement
		}

		/// <summary>
		/// Completely removes this player from the DB.
		/// </summary>
		/// <param name="playerToken">Unique token of the requesting player</param>
		/// <returns>Success/Failure</returns>
		public bool RemovePlayer(int playerToken)
		{
			throw new NotImplementedException(); // TODO Implement
		}

		/// <summary>
		/// Adds this player to the waiting queue. And creates a new match if conditions apply.
		/// </summary>
		/// <param name="playerToken">Unique token of the requesting player</param>
		/// <returns>Success/Failure</returns>
		public bool SubmitPlayerTicket(int playerToken)
		{
			throw new NotImplementedException(); // TODO Implement
			CreateMatch(/*<Player IDs>*/); // Call this asynchronously if now 2 players are waiting in the queue
		}

		/// <summary>
		/// Removes this player from the waiting queue.
		/// </summary>
		/// <param name="playerToken">Unique token of the requesting player</param>
		/// <returns>Success/Failure</returns>
		public bool RemovePlayerTicket(int playerToken)
		{
			throw new NotImplementedException(); // TODO Implement
		}

		/// <summary>
		/// Adds this player to the match.
		/// </summary>
		/// <param name="playerToken">Unique token of the requesting player</param>
		/// <param name="matchID">Unique ID of the match this play joins</param>
		/// <returns>Success/Failure</returns>
		public bool JoinMatch(int playerToken, int matchID)
		{
			throw new NotImplementedException();
			//StartMatch(/*<MatchID>*/); // Call this asynchronously if now 2 players have agreed to start the match
		}

		/// <summary>
		/// Remove this player from its current the match.
		/// </summary>
		/// <param name="playerToken">Unique token of the requesting player</param>
		/// <returns>Success/Failure</returns>
		public bool LeaveMatch(int playerToken)
		{
			throw new NotImplementedException();
			// If this is detrimental to the match, it should probably trigger a match closure
		}

		/// <summary>
		/// Registers this player's answer as their answer to their current question
		/// </summary>
		/// <param name="playerToken">Unique token of the answering player</param>
		/// <param name="answerID">ID (Number between 1-4) of the answer to register</param>
		/// <returns>Success/Failure</returns>
		/// <exception cref="NotImplementedException"></exception>
		public bool RegisterAnswer(int playerToken, int answerID)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Creates a new match for the players listed in playerIDs. And adds them to it.
		/// </summary>
		/// <param name="playerIDs">IDs of the players to add</param>
		/// <returns>Success/Failure</returns>
		private bool CreateMatch(params int[] playerIDs)
		{
			throw new NotImplementedException(); // TODO Implement
		}

		/// <summary>
		/// Starts the match listed in MatchID
		/// </summary>
		/// <param name="matchID">Unique ID of the match to start</param>
		/// <returns>Success/Failure</returns>
		private bool StartMatch(int matchID)
		{
			throw new NotImplementedException(); // TODO Implement
		}
		#endregion
		#endregion
		#endregion
	}
}
