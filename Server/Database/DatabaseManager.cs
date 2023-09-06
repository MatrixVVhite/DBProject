using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.X509.Qualified;
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
            string query = $"SELECT PlayerToken FROM players WHERE PlayerToken = {playerToken};";
            var test = ExecuteQuery(query);
			if (test.Count == 0) { return false; }
			else { return true; }
        }

		/// <summary>
		/// Returns whether this player has a ticket in the waiting queue.
		/// </summary>
		/// <param name="playerToken">Unique token of the requesting player</param>
		/// <returns>Whether this player has a ticket</returns>
		public bool GetTicketExists(int playerToken)
		{
            string getPlayerQuery = $"SELECT PlayerID FROM players WHERE PlayerToken = {playerToken};";
            int playerID = int.Parse(ExecuteQuery(getPlayerQuery)["PlayerID"].ToString());
            string query = $"SELECT PlayerID FROM queue WHERE PlayerID = {playerID};";
            var test = ExecuteQuery(query);
            if (test.Count == 0) { return false; }
            else { return true; }
		}

		/// <summary>
		/// Returns whether the server has found a match for this player. And if it did - the match ID.
		/// </summary>
		/// <param name="playerToken">Unique token of the requesting player</param>
		/// <returns>Whether the server has found a match</returns>
		public bool GetMatchFound(int playerToken)
		{
			throw new NotImplementedException(); //TODO Implement
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
			throw new NotImplementedException(); //TODO Implement
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
			throw new NotImplementedException(); //TODO Implement
		}
		#endregion

		#region INSERT/UPDATE/DELETE
		private int ExecuteInsertUpdate(string statement)
		{
			Debug.Assert(statement.ToLower().Contains("insert") || statement.ToLower().Contains("update") || statement.ToLower().Contains("delete"));
			int rowsAffected = 0;
			if (TryConnect())
			{
				try
				{
					rowsAffected = ExecuteCommand(statement);
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

		private int GetPlayerID(int playerToken)
		{
            string getPlayerQuery = $"SELECT PlayerID FROM players WHERE PlayerToken = {playerToken};";
            return int.Parse(ExecuteQuery(getPlayerQuery)["PlayerID"].ToString());
        }

		private bool TestTwoStatements(string statement1, string statement2)
		{
            bool test1 = ExecuteInsertUpdate(statement1) != 0;
            bool test2 = ExecuteInsertUpdate(statement2) > 0;
			return test1 && test2;
        }

		/// <summary>
		/// Connects this player to the server and adds them to the Players table.
		/// </summary>
		/// <param name="playerToken">Registers a unique token for the player to identify with</param>
		/// <param name="playerName">Registers this name as the player's alias</param>
		/// <returns>Success/Failure</returns>
		public bool AddNewPlayer(int playerToken, string playerName)
		{
            string statement = $"INSERT INTO `finalprojectdb`.`players` (`PlayerName`, `PlayerToken`) VALUES " +
				$"('{playerName}', '{playerToken}');";
            return ExecuteInsertUpdate(statement) > 0;
        }

		/// <summary>
		/// Completely removes this player from the DB.
		/// </summary>
		/// <param name="playerToken">Unique token of the requesting player</param>
		/// <returns>Success/Failure</returns>
		public bool RemovePlayer(int playerToken)
		{
			string statement = $"DELETE FROM `finalprojectdb`.`players` WHERE(`PlayerToken` = '{playerToken}');";
			return ExecuteInsertUpdate(statement) > 0;
        }

		/// <summary>
		/// Adds this player to the waiting queue. And creates a new match if conditions apply.
		/// </summary>
		/// <param name="playerToken">Unique token of the requesting player</param>
		/// <returns>Success/Failure</returns>
		public bool SubmitPlayerTicket(int playerToken)
        {
			int playerID = GetPlayerID(playerToken);
            string statement1 = $"INSERT IGNORE INTO `finalprojectdb`.`queue` (`PlayerID`) VALUES ('{playerID}');";
            string statement2 = $"UPDATE `finalprojectdb`.`players` SET `PlayerStatus` = '1' WHERE (`PlayerID` = '{playerID}');";

            string getPlayersInQueue = $"SELECT COUNT(PlayerID) FROM finalprojectdb.queue;";
            if (int.Parse(ExecuteQuery(getPlayersInQueue)["COUNT(PlayerID)"].ToString()) > 1)//TODO Finish create match implementation and fix queue check
            {//To get player IDs take this players ID, and the player who is at the top of the queue, ignoring the original player
                CreateMatch(/*<Player IDs>*/);
            }
			return TestTwoStatements(statement1, statement2); //TODO returns false when should be true, fix that to show it works fine
        }

		/// <summary>
		/// Removes this player from the waiting queue.
		/// </summary>
		/// <param name="playerToken">Unique token of the requesting player</param>
		/// <returns>Success/Failure</returns>
		public bool RemovePlayerTicket(int playerToken)
		{
            int playerID = GetPlayerID(playerToken);
            string statement1 = $"DELETE FROM `finalprojectdb`.`queue` WHERE(`PlayerID` = '{playerID}');";
			string statement2 = $"UPDATE `finalprojectdb`.`players` SET `PlayerStatus` = '0' WHERE (`PlayerID` = '{playerID}');";
            return TestTwoStatements(statement1, statement2); //TODO returns false when should be true, fix that to show it works fine
        }

		/// <summary>
		/// Adds this player to the match.
		/// </summary>
		/// <param name="playerToken">Unique token of the requesting player</param>
		/// <returns>Success/Failure</returns>
		public bool JoinMatch(int playerToken)
		{
            int playerID = GetPlayerID(playerToken);
            string statement = $"UPDATE `finalprojectdb`.`queue` SET `AcceptMatch` = '1' WHERE (`PlayerID` = '{playerID}');";
			return ExecuteInsertUpdate(statement) > 0;
            //Each player will send a JoinMatch(playerToken) check when they click accept match in game,
			//when either of them do so try to use StartMatch(MatchID). Once both have agreed, it will start
			//TODO Finish implementation
        }

		/// <summary>
		/// Remove this player from its current match.
		/// </summary>
		/// <param name="playerToken">Unique token of the requesting player</param>
		/// <returns>Success/Failure</returns>
		public bool LeaveMatch(int playerToken)
		{
            int playerID = GetPlayerID(playerToken);
            string statement1 = $"DELETE FROM `finalprojectdb`.`queue` WHERE (`PlayerID` = '{playerID}');";
            string statement2 = $"UPDATE `finalprojectdb`.`players` SET `PlayerStatus` = '0' WHERE (`PlayerID` = '{playerID}');";
            return TestTwoStatements(statement1, statement2); //TODO Get the match the player was in and use EndMatch(matchID)
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
			throw new NotImplementedException(); //TODO Implement
		}

		/// <summary>
		/// Creates a new match for the players listed in playerIDs. And adds them to it.
		/// </summary>
		/// <param name="playerIDs">IDs of the players to add</param>
		/// <returns>Success/Failure</returns>
		private bool CreateMatch(params int[] playerIDs)
		{
            string statement1 = $"INSERT INTO `finalprojectdb`.`lobbies` (`Player1ID`, `Player2ID`) VALUES ('{playerIDs[0]}', '{playerIDs[1]}');";
			bool test1 = ExecuteInsertUpdate(statement1) > 0;
            string findLobbyQuery = $"SELECT Player1ID FROM lobbies WHERE Player1ID = {playerIDs[0]};";
			int lobbyNumber = int.Parse(ExecuteQuery(findLobbyQuery)["LobbyID"].ToString()); //Creates and returns the lobby number

            string statement2 = $"UPDATE `finalprojectdb`.`players` SET `PlayerStatus` = '2', `LobbyNumber` = '{lobbyNumber}' WHERE (`PlayerID` = '{playerIDs[0]}');";
            string statement3 = $"UPDATE `finalprojectdb`.`players` SET `PlayerStatus` = '2', `LobbyNumber` = '{lobbyNumber}' WHERE (`PlayerID` = '{playerIDs[1]}');";
            //TODO Send both players join request here
            return TestTwoStatements(statement2, statement3) && test1;// TODO finish checkup
        }

		/// <summary>
		/// Starts the match listed in MatchID
		/// </summary>
		/// <param name="matchID">Unique ID of the match to start</param>
		/// <returns>Success/Failure</returns>
		private bool StartMatch(int matchID)
		{
            //Get both players in the lobby. If both players signal 1 on queue - AcceptMatch, use RemovePlayerTicket(int playerToken) on both players,
			//update both players' status to 2, and go through with the match
            //If 1 player leaves or doesn't handshake in time, use EndMatch(matchID) and use SubmitPlayerTicket(int playerToken) on the active player
            //Then use RemovePlayerTicket(int playerToken) on the inactive player
            string statement = $"UPDATE `finalprojectdb`.`lobbies` SET `IsGameActive` = '1' WHERE (`LobbyID` = '{matchID}');";
            return ExecuteInsertUpdate(statement) > 0; // TODO finish implementation
        }

		private bool EndMatch(int matchID)
		{
			//Get both players who were in the match. Update their lobby number to NULL, reset their points, set their status to 0 and bring them back to main menu
			throw new NotImplementedException();
		}
		#endregion
		#endregion
		#endregion
	}
}
