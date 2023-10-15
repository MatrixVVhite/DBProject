using System.Data;
using System.Diagnostics;
using MySql.Data.MySqlClient;
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
			string query = $"SELECT PlayerToken FROM players WHERE PlayerToken = {playerToken} LIMIT 1;";
			var result = ExecuteQuery(query);
			return result.Count != 0;
		}

		/// <summary>
		/// Returns whether this player has a ticket in the waiting queue.
		/// </summary>
		/// <param name="playerToken">Unique token of the requesting player</param>
		/// <returns>Whether this player has a ticket</returns>
		public bool GetTicketExists(int playerToken)
		{
			string getPlayerQuery = $"SELECT PlayerID FROM players WHERE PlayerToken = {playerToken} LIMIT 1;";
			int playerID = int.Parse(ExecuteQuery(getPlayerQuery)["PlayerID"].ToString());
			string query = $"SELECT PlayerID FROM queue WHERE PlayerID = {playerID} LIMIT 1;";
			var result = ExecuteQuery(query);
			return result.Count != 0;
		}

		/// <summary>
		/// Returns whether the server has found a match for this player.
		/// Pings every few seconds if the player is in queue.
		/// If returns anything but 0 send the player a join request
		/// </summary>
		/// <param name="playerToken">Unique token of the requesting player</param>
		/// <returns>Whether the server has found a match by returning the player's match ID, !=0 == true</returns>
		public int GetMatchFound(int playerToken)
		{
			int playerID = GetPlayerID(playerToken);
			int playerLobby = GetPlayerLobby(playerID);

			return playerLobby;
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
			int player1ID = GetPlayer1IDFromLobby(matchID);
			int player2ID = GetPlayer2IDFromLobby(matchID);
			int player1Score = GetPlayerScore(player1ID);
			int player2Score = GetPlayerScore(player2ID);
			string getP1CurrentQuestion = $"SELECT CurrentQuestion FROM `session stats` WHERE PlayerID = {player1ID};";
			int currentP1QuestionID = int.Parse(ExecuteQuery(getP1CurrentQuestion)["CurrentQuestion"].ToString());
			string getP2CurrentQuestion = $"SELECT CurrentQuestion FROM `session stats` WHERE PlayerID = {player2ID};";
			int currentP2QuestionID = int.Parse(ExecuteQuery(getP2CurrentQuestion)["CurrentQuestion"].ToString());
			int p1QuestionsAnswered = currentP1QuestionID - 1;
			int p2QuestionsAnswered = currentP2QuestionID - 1;
			int p1QuestionsLeft = 10 - p1QuestionsAnswered;
			int p2QuestionsLeft = 10 - p2QuestionsAnswered;
			string getGameActiveStatus = $"SELECT IsGameActive FROM lobbies WHERE LobbyID = {matchID};";
			int gameActiveStatus = int.Parse(ExecuteQuery(getGameActiveStatus)["IsGameActive"].ToString());
			var dataToSend = new JsonDict()
			{
				{ "P1Score", player1Score },
				{ "P2Score", player2Score },
				{ "P1QuestionsAnswered", p1QuestionsAnswered },
				{ "P2QuestionsAnswered", p2QuestionsAnswered },
				{ "P1QuestionsLeft", p1QuestionsLeft },
				{ "P2QuestionsLeft", p2QuestionsLeft},
				{ "GameActiveStatus", gameActiveStatus }
			};
			return dataToSend;
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
			int playerID = GetPlayerID(playerToken);
			string getCurrentQuestion = $"SELECT CurrentQuestion FROM `session stats` WHERE PlayerID = {playerID};";
			int currentQuestionID = int.Parse(ExecuteQuery(getCurrentQuestion)["CurrentQuestion"].ToString());
			return GetQuestion(currentQuestionID);
		}

		private int GetPlayerScore(int playerID)
		{
			string getPlayerScoreQuery = $"SELECT Score FROM `session stats` WHERE PlayerID = {playerID};";
			return int.Parse(ExecuteQuery(getPlayerScoreQuery)["Score"].ToString());
		}

		private int GetPlayerID(int playerToken)
		{
			string getPlayerQuery = $"SELECT PlayerID FROM players WHERE PlayerToken = {playerToken};";
			return int.Parse(ExecuteQuery(getPlayerQuery)["PlayerID"].ToString());
		}

		private int GetPlayerToken(int playerID)
		{
			string getPlayerQuery = $"SELECT PlayerToken FROM players WHERE PlayerID = {playerID};";
			return int.Parse(ExecuteQuery(getPlayerQuery)["PlayerToken"].ToString());
		}

		private int GetPlayerLobby(int playerID)
		{
			string getLobbyQuery = $"SELECT LobbyID FROM lobbies WHERE (Player1ID = {playerID}) OR (Player2ID = {playerID});";
			try { return int.Parse(ExecuteQuery(getLobbyQuery)["LobbyID"].ToString()); }
			catch (Exception ex) { return 0; };
		}

        private int GetPlayer1IDFromLobby(int LobbyID)
		{
			string getPlayer1Query = $"SELECT Player1ID FROM lobbies WHERE LobbyID = {LobbyID};";
			return int.Parse(ExecuteQuery(getPlayer1Query)["Player1ID"].ToString());
		}

		private int GetPlayer2IDFromLobby(int LobbyID)
		{
			string getPlayer2Query = $"SELECT Player2ID FROM lobbies WHERE LobbyID = {LobbyID};";
			return int.Parse(ExecuteQuery(getPlayer2Query)["Player2ID"].ToString());
		}

		private bool GetHandshakeStatusFromLobby(int lobbyID)
		{
			int player1ID = GetPlayer1IDFromLobby(lobbyID);
			int player2ID = GetPlayer2IDFromLobby(lobbyID);
			string getP1Handshake = $"SELECT AcceptMatch FROM queue WHERE PlayerID = {player1ID};";
			string getP2Handshake = $"SELECT AcceptMatch FROM queue WHERE PlayerID = {player2ID};";
			int testP1Handshake = int.Parse(ExecuteQuery(getP1Handshake)["AcceptMatch"].ToString());
			int testP2Handshake = int.Parse(ExecuteQuery(getP2Handshake)["AcceptMatch"].ToString());
			if (testP1Handshake == 1 && testP2Handshake == 1)
			{
				return true;
			}
			else { return false; }
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

		private bool UpdatePlayerStatus(int PlayerID, int newStatus) //New status must be 0, 1 or 2
		{
			string statement = $"UPDATE players SET PlayerStatus = {newStatus} WHERE (PlayerID = {PlayerID});";
			return ExecuteInsertUpdate(statement) > 0;
		}

		private bool RemovePlayerStats(int playerID)
		{
			string removeSessionStats = $"DELETE FROM `session stats` WHERE(PlayerID = {playerID});";
			bool test = ExecuteInsertUpdate(removeSessionStats) > 0;
			return test;
		}

		private bool RemovePlayerLobbyNumber(int playerID)
		{
			string statement = $"UPDATE players SET LobbyNumber = 0 WHERE (PlayerID = {playerID});";
			return ExecuteInsertUpdate(statement) > 0;
		}

		private bool RemoveLobby(int lobbyID)
		{
            string deleteLobby = $"DELETE FROM lobbies WHERE (LobbyID = {lobbyID});";
			return ExecuteInsertUpdate(deleteLobby) > 0;
        }

        private bool Add1ToCurrentQuestion(int playerID)
        {
            string getCurrentQuestion = $"SELECT CurrentQuestion FROM `session stats` WHERE PlayerID = {playerID};";
            int currentQuestionNum = int.Parse(ExecuteQuery(getCurrentQuestion)["CurrentQuestion"].ToString());
			string add1ToCurrentQuestionStatement = $"UPDATE `session stats` SET CurrentQuestion = {currentQuestionNum + 1} WHERE (PlayerID = {playerID});";
			return ExecuteInsertUpdate(add1ToCurrentQuestionStatement) > 0;
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
			string statement = $"INSERT INTO players (PlayerName, PlayerToken) VALUES " +
				$"({playerName}, {playerToken});";
			return ExecuteInsertUpdate(statement) > 0;
		}

		/// <summary>
		/// Completely removes this player from the DB.
		/// </summary>
		/// <param name="playerToken">Unique token of the requesting player</param>
		/// <returns>Success/Failure</returns>
		public bool RemovePlayer(int playerToken)
		{
			int playerID = GetPlayerID(playerToken);
            int playerLobby;
            if (GetPlayerLobby(playerID) != 0) { playerLobby = GetPlayerLobby(playerID); }
            else { playerLobby = 0; }
            if (playerLobby != 0)
            {
                int player1ID = GetPlayer1IDFromLobby(playerLobby);
                int player2ID = GetPlayer2IDFromLobby(playerLobby);
                if (player1ID == playerID) 
				{ 
					RemovePlayerStats(player1ID);
					RemovePlayerStats(player2ID);
					SubmitPlayerTicket(GetPlayerToken(player2ID));
                    UpdatePlayerStatus(player2ID, 1);
                }
                else 
				{
					RemovePlayerStats(player1ID);
					RemovePlayerStats(player2ID);
					SubmitPlayerTicket(GetPlayerToken(player1ID));
					UpdatePlayerStatus(player1ID, 1);
                }
                RemoveLobby(playerLobby);
            }
            string deleteFromQueueQuery = $"DELETE FROM queue WHERE(PlayerID = {playerID});";
            bool deleteFromQueue = ExecuteInsertUpdate(deleteFromQueueQuery) > 0;
            string deleteFromDBStatement = $"DELETE FROM players WHERE(PlayerToken = {playerToken});";
			bool deleteFromDB = ExecuteInsertUpdate(deleteFromDBStatement) > 0;
			return deleteFromQueue && deleteFromDB;
		}

		/// <summary>
		/// Adds this player to the waiting queue. And creates a new match if conditions apply.
		/// </summary>
		/// <param name="playerToken">Unique token of the requesting player</param>
		/// <returns>Success/Failure</returns>
		public bool SubmitPlayerTicket(int playerToken)
		{
			int playerID = GetPlayerID(playerToken);
			bool removeLobbyNumber = RemovePlayerLobbyNumber(playerID);
			string insertIntoQueueStatement = $"INSERT IGNORE INTO queue (`PlayerID`) VALUES ({playerID});";
			bool insertIntoQueue = ExecuteInsertUpdate(insertIntoQueueStatement) > 0;
			bool updateStatus = UpdatePlayerStatus(playerID, 1);

			string getLFGPlayersInQueue = $"SELECT COUNT(queue.PlayerID) FROM queue INNER JOIN " +
				$"players ON queue.PlayerID = players.PlayerID WHERE AcceptMatch = 0 AND LobbyNumber = 0;";
			if (int.Parse(ExecuteQuery(getLFGPlayersInQueue)["COUNT(queue.PlayerID)"].ToString()) > 1)
			{
				string getPlayer2Query = $"SELECT queue.PlayerID FROM queue INNER JOIN players ON " +
					$"queue.PlayerID = players.PlayerID WHERE AcceptMatch = 0 AND LobbyNumber = 0 AND " +
					$"queue.PlayerID != {playerID} LIMIT 1;";
				int player2ID = int.Parse(ExecuteQuery(getPlayer2Query)["PlayerID"].ToString());
				int[] players = new int[2] { playerID, player2ID };
				CreateMatch(players);
			}
			return removeLobbyNumber && insertIntoQueue && updateStatus;
		}

		/// <summary>
		/// Removes this player from the waiting queue.
		/// </summary>
		/// <param name="playerToken">Unique token of the requesting player</param>
		/// <returns>Success/Failure</returns>
		public bool RemovePlayerTicket(int playerToken)
		{
			int playerID = GetPlayerID(playerToken);
			int playerLobby;
			if (GetPlayerLobby(playerID) != 0) { playerLobby = GetPlayerLobby(playerID); }
			else { playerLobby = 0; }
			if (playerLobby != 0)
			{
				int player1ID = GetPlayer1IDFromLobby(playerLobby);
				int player2ID = GetPlayer2IDFromLobby(playerLobby);
				if (player1ID == playerID) 
				{
					SubmitPlayerTicket(GetPlayerToken(player2ID));
                    UpdatePlayerStatus(player2ID, 1);
                }
				else 
				{ 
					SubmitPlayerTicket(GetPlayerToken(player1ID));
                    UpdatePlayerStatus(player1ID, 1);
                }
                RemoveLobby(playerLobby);
            }
			string deleteFromQueueQuery = $"DELETE FROM queue WHERE(PlayerID = {playerID});";
			bool deleteFromQueue = ExecuteInsertUpdate(deleteFromQueueQuery) > 0;
			bool updateStatus = UpdatePlayerStatus (playerID, 0);
			return deleteFromQueue && updateStatus;
		}

		public bool RemovePlayerTicketByID(int playerID)
		{
			string deleteFromQueue = $"DELETE FROM queue WHERE(PlayerID = {playerID});";
			bool test1 = ExecuteInsertUpdate(deleteFromQueue) > 0;
			bool test2 = UpdatePlayerStatus(playerID, 0);
			return test1 && test2;
		}

		/// <summary>
		/// Player sends this when they accept the match in-game
		/// Adds this player to the match.
		/// </summary>
		/// <param name="playerToken">Unique token of the requesting player</param>
		/// <returns>Success/Failure</returns>
		public bool JoinMatch(int playerToken)
		{
			int playerID = GetPlayerID(playerToken);
			string statement = $"UPDATE queue SET AcceptMatch = 1 WHERE (PlayerID = {playerID});";
			bool test1 = ExecuteInsertUpdate(statement) > 0;
			int lobbyID = GetPlayerLobby(playerID);
			if (GetHandshakeStatusFromLobby(lobbyID))
			{
				bool test2 = StartMatch(lobbyID);
				return test1 && test2;
			}
			else { return test1; }
			//Each player will send a JoinMatch(playerToken) check when they click accept match in game,
			//when either of them do so try to use StartMatch(MatchID). Once both have agreed, it will start
		}

		/// <summary>
		/// Remove this player from its current match.
		/// </summary>
		/// <param name="playerToken">Unique token of the requesting player</param>
		/// <returns>Success/Failure</returns>
		public bool LeaveMatch(int playerToken)
		{
			int playerID = GetPlayerID(playerToken);
			int lobbyID = GetPlayerLobby(playerID);
			return EndMatch(lobbyID);
		}

		/// <summary>
		/// Registers this player's answer as their answer to their current question
		/// </summary>
		/// <param name="playerToken">Unique token of the answering player</param>
		/// <param name="answerID">ID (Number between 1-4) of the answer to register</param>
		/// <returns>Success/Failure</returns>
		public bool RegisterAnswer(int playerToken, int answerID)
		{
			int playerID = GetPlayerID(playerToken);
			string getCorrectAnswerQuery = $"SELECT questions.CorrectAnswer FROM questions INNER JOIN " +
				$"`session stats` ON questions.QuestionID = `session stats`.CurrentQuestion WHERE " +
				$"`session stats`.PlayerID = {playerID};";
			try
			{
				if (answerID == int.Parse(ExecuteQuery(getCorrectAnswerQuery)["CorrectAnswer"].ToString())) 
				{
					Add1ToCurrentQuestion(playerID);
					return true; 
				}
				else 
				{
					Add1ToCurrentQuestion(playerID);
					return false;
				}
			}
			catch (Exception ex) 
			{
				Add1ToCurrentQuestion(playerID);
				return false;
			}
		}

		/// <summary>
		/// Creates a new match for the players listed in playerIDs. And adds them to it.
		/// </summary>
		/// <param name="playerIDs">IDs of the players to add</param>
		/// <returns>Success/Failure</returns>
		private bool CreateMatch(params int[] playerIDs)
		{
			string statement1 = $"INSERT INTO lobbies (Player1ID, Player2ID) VALUES ({playerIDs[0]}, {playerIDs[1]});";
			bool test1 = ExecuteInsertUpdate(statement1) > 0;
			int lobbyNumber = GetPlayerLobby(playerIDs[0]); //Creates and returns the lobby number
			string statement2 = $"INSERT INTO `session stats` (PlayerID, LobbyID) VALUES ({playerIDs[0]}, {lobbyNumber});";
			string statement3 = $"INSERT INTO `session stats` (PlayerID, LobbyID) VALUES ({playerIDs[1]}, {lobbyNumber});";
			string statement4 = $"UPDATE players SET PlayerStatus = 2, LobbyNumber = {lobbyNumber} WHERE (PlayerID = {playerIDs[0]});";
			string statement5 = $"UPDATE players SET PlayerStatus = 2, LobbyNumber = {lobbyNumber} WHERE (PlayerID = {playerIDs[1]});";
			return test1 && TestTwoStatements(statement2, statement3) && TestTwoStatements(statement4, statement5);
		}

		/// <summary>
		/// Starts the match listed in MatchID
		/// </summary>
		/// <param name="matchID">Unique ID of the match to start</param>
		/// <returns>Success/Failure</returns>
		private bool StartMatch(int matchID)
		{
			//Get both players in the lobby. If both players signal 1 on queue - AcceptMatch, update both players' status to 2, and go through with the match
			//If 1 player leaves or doesn't handshake in time, use EndMatch(matchID) and use SubmitPlayerTicket(int playerToken) on the active player
			//Then use RemovePlayerTicket(int playerToken) on the inactive player
			int player1ID = GetPlayer1IDFromLobby(matchID);
			int player2ID = GetPlayer2IDFromLobby(matchID);
			string statement = $"UPDATE lobbies SET IsGameActive = 1 WHERE (LobbyID = {matchID});";
			bool test1 = ExecuteInsertUpdate(statement) > 0;
			bool test2 = UpdatePlayerStatus(player1ID, 2);
			bool test3 = UpdatePlayerStatus(player2ID, 2);
			return test1 && test2 && test3;
		}

		private bool EndMatch(int matchID)
		{
			int player1ID = GetPlayer1IDFromLobby(matchID);
			int player2ID = GetPlayer2IDFromLobby(matchID);
			bool test1 = RemovePlayerStats(player1ID);
			bool test2 = RemovePlayerStats(player2ID);
			bool test3 = RemovePlayerLobbyNumber(player1ID);
			bool test4 = RemovePlayerLobbyNumber(player2ID);
			bool test5 = RemovePlayerTicketByID(player1ID);
			bool test6 = RemovePlayerTicketByID(player2ID);
			bool test7 = RemoveLobby(matchID);
			return test1 && test2 && test3 && test4 && test5 && test6 && test7;
		}
		#endregion
		#endregion
		#endregion
	}
}
