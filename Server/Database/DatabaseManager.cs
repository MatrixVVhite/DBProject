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
		#endregion

		#region SINGLETON_PROPERTIES
		private static readonly Lazy<DatabaseManager> _lazy = new Lazy<DatabaseManager>(() => new DatabaseManager());

		public static DatabaseManager Instance { get => _lazy.Value; }
		#endregion
		#endregion

		#region METHODS
		#region UTILITY
		#region CREATE_COMMAND
		private static MySqlCommand CreateCommand(string statement, MySqlConnection conn)
		{
			var cmd = conn.CreateCommand();
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

		#region STATEMENTS
		private static int ExecuteCommand(string statement)
		{
			Debug.Assert(statement.ToLower().Contains("insert") || statement.ToLower().Contains("update") || statement.ToLower().Contains("delete"));
			using var conn = new MySqlConnection(CON_STR);
			int rowsAffected = 0;
			conn.Open();
			try
			{
				rowsAffected = CreateCommand(statement, conn).ExecuteNonQuery();
			}
			catch (MySqlException ex)
			{
				Debug.WriteLine(ex);
			}
			return rowsAffected;
		}

		private static JsonDict ExecuteQuery(string query)
		{
			Debug.Assert(query.ToLower().Contains("select"));
			var dt = new DataTable();
			using var conn = new MySqlConnection(CON_STR);
			MySqlDataAdapter dataAdapter = new(query, conn);
			dataAdapter.SelectCommand.CommandType = CommandType.Text;
			conn.Open();
			try
			{
				dataAdapter.Fill(dt);
			}
			catch (MySqlException ex)
			{
				Debug.WriteLine(ex);
			}

			return DataTableToDictionary(dt);
		}

		#region UNSAFE
		private string ExecuteQueryString(string query, string? key = null)
		{
			JsonDict dict = ExecuteQuery(query);
			return (key == null ? dict.ElementAt(0).Value : dict[key]).ToString();
		}

		private int ExecuteQueryInt(string query, string? key = null)
		{
			return int.Parse(ExecuteQueryString(query, key));
		}

		private bool ExecuteQueryBool(string query, string? key = null)
		{
			return ExecuteQueryInt(query, key) != 0;
		}
		#endregion

		#region SAFE
		private bool ExecuteQueryExists(string query, string? key = null)
		{
			return TryExecuteQuery(query, out bool _, key);
		}

		private bool TryExecuteQuery(string query, ref string val, string? key = null)
		{
			val = string.Empty;
			try
			{
				val = ExecuteQueryString(query, key);
				return true;
			}
			catch
			{
				return false;
			}
		}

		private bool TryExecuteQuery(string query, out int val, string? key = null)
		{
			val = 0;
			try
			{
				val = ExecuteQueryInt(query, key);
				return true;
			}
			catch
			{
				return false;
			}
		}

		private bool TryExecuteQuery(string query, out bool val, string? key = null)
		{
			val = false;
			try
			{
				val = ExecuteQueryBool(query, key);
				return true;
			}
			catch
			{
				return false;
			}
		}
		#endregion
		#endregion
		#endregion

		#region MYSQL_STATEMENTS
		#region STRING_ONLY
		string QueryPlayerID(int playerToken) => $"SELECT PlayerID FROM players WHERE PlayerToken = {playerToken} LIMIT 1";

		string QueryPlayerToken(int playerID) => $"SELECT PlayerToken FROM players WHERE PlayerID = {playerID} LIMIT 1";
		#endregion

		#region QUERIES
		public JsonDict GetQuestion(int questionID)
		{
			string query = $"SELECT * FROM questions WHERE QuestionID = {questionID};";
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
			return ExecuteQueryExists(query);
		}

		/// <summary>
		/// Returns whether this player has a ticket in the waiting queue.
		/// </summary>
		/// <param name="playerToken">Unique token of the requesting player</param>
		/// <returns>Whether this player has a ticket</returns>
		public bool GetTicketExists(int playerToken)
		{
			string query = $"SELECT PlayerID FROM queue WHERE PlayerID = ({QueryPlayerID(playerToken)}) LIMIT 1;";
			return ExecuteQueryExists(query);
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
		/// Returns whether this player is inside a match.
		/// Pings every few seconds if the player is waiting for other clients to accept.
		/// </summary>
		/// <param name="playerToken">Unique token of the requesting player</param>
		/// <returns>Whether this player is inside a match</returns>
		public bool GetMatchActive(int playerToken)
		{
			int matchID = GetMatchFound(playerToken);
			if (matchID != 0)
			{
				string getGameActiveStatus = $"SELECT IsGameActive FROM lobbies WHERE LobbyID = {matchID};";
				bool gameActive = (MatchStatus)ExecuteQueryInt(getGameActiveStatus) != MatchStatus.Inactive;
				return gameActive;
			}
			else
			{
				return false;
			}
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
		public JsonDict GetMatchStatus(int matchID, int playerToken)
		{
			if (ExecuteQueryExists($"SELECT LobbyID FROM lobbies WHERE LobbyID = {matchID};"))
			{
				int callerID = GetPlayerID(playerToken);
				int player1ID = GetPlayer1IDFromLobby(matchID);
				int player2ID = GetPlayer2IDFromLobby(matchID);
				int callerIs = callerID == player1ID ? 1 : 2;
				int player1Score = GetPlayerScore(player1ID);
				int player2Score = GetPlayerScore(player2ID);
				string getP1CurrentQuestion = $"SELECT CurrentQuestion FROM `session stats` WHERE PlayerID = {player1ID};";
				int currentP1QuestionID = ExecuteQueryInt(getP1CurrentQuestion);
				string getP2CurrentQuestion = $"SELECT CurrentQuestion FROM `session stats` WHERE PlayerID = {player2ID};";
				int currentP2QuestionID = ExecuteQueryInt(getP2CurrentQuestion);
				int p1QuestionsAnswered = currentP1QuestionID - 1;
				int p2QuestionsAnswered = currentP2QuestionID - 1;
				int p1QuestionsLeft = 10 - p1QuestionsAnswered;
				int p2QuestionsLeft = 10 - p2QuestionsAnswered;
				string getGameActiveStatus = $"SELECT IsGameActive FROM lobbies WHERE LobbyID = {matchID};";
				int gameActiveStatus = ExecuteQueryInt(getGameActiveStatus);
				var dataToSend = new JsonDict()
			{
				{ "YouAre", callerIs },
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
			else
			{
				return new JsonDict();
			}
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
			if (TryExecuteQuery(getCurrentQuestion, out int currentQuestionID) && currentQuestionID <= 10)
				return GetQuestion(currentQuestionID);
			else
				return new JsonDict();
		}

		private int GetPlayerScore(int playerID)
		{
			string getPlayerScoreQuery = $"SELECT Score FROM `session stats` WHERE PlayerID = {playerID};";
			return ExecuteQueryInt(getPlayerScoreQuery);
		}

		private int GetPlayerID(int playerToken)
		{
			string getPlayerQuery = $"{QueryPlayerID(playerToken)};"; // TODO Test if I need this line
			return ExecuteQueryInt(getPlayerQuery);
		}

		private int GetPlayerToken(int playerID)
		{
			string getPlayerQuery = $"{QueryPlayerToken(playerID)};"; // TODO Test if I need this line
			return ExecuteQueryInt(getPlayerQuery);
		}

		private int GetPlayerLobby(int playerID)
		{
			string getLobbyQuery = $"SELECT LobbyID FROM lobbies WHERE (Player1ID = {playerID}) OR (Player2ID = {playerID});";
			if (TryExecuteQuery(getLobbyQuery, out int lobby))
				return lobby;
			else
				return 0;
		}

		private int GetPlayer1IDFromLobby(int lobbyID)
		{
			string getPlayer1Query = $"SELECT Player1ID FROM lobbies WHERE LobbyID = {lobbyID};";
			return ExecuteQueryInt(getPlayer1Query);
		}

		private int GetPlayer2IDFromLobby(int lobbyID)
		{
			string getPlayer2Query = $"SELECT Player2ID FROM lobbies WHERE LobbyID = {lobbyID};";
			return ExecuteQueryInt(getPlayer2Query);
		}

		private bool GetHandshakeStatusFromLobby(int lobbyID)
		{
			int player1ID = GetPlayer1IDFromLobby(lobbyID);
			int player2ID = GetPlayer2IDFromLobby(lobbyID);
			string getP1Handshake = $"SELECT AcceptMatch FROM queue WHERE PlayerID = {player1ID};";
			string getP2Handshake = $"SELECT AcceptMatch FROM queue WHERE PlayerID = {player2ID};";
			bool testP1Handshake = ExecuteQueryBool(getP1Handshake);
			bool testP2Handshake = ExecuteQueryBool(getP2Handshake);
			return testP1Handshake & testP2Handshake;
		}
		#endregion

		#region COMMAND
		private bool UpdatePlayerStatus(int PlayerID, PlayerStatus newStatus)
		{
			string statement = $"UPDATE players SET PlayerStatus = {(int)newStatus} WHERE (PlayerID = {PlayerID});";
			return ExecuteCommand(statement) > 0;
		}

		private bool RemovePlayerStats(int playerID)
		{
			string removeSessionStats = $"DELETE FROM `session stats` WHERE(PlayerID = {playerID});";
			bool test = ExecuteCommand(removeSessionStats) > 0;
			return test;
		}

		private bool AcceptMatchTo0(int playerID)
		{
            string acceptMatchTo0Statement = $"UPDATE queue SET AcceptMatch = 0 WHERE (PlayerID = {playerID}) LIMIT 1;";
            return ExecuteCommand(acceptMatchTo0Statement) > 0;
        }

		private bool ChangeGameActiveStatus(int lobbyID, MatchStatus newStatus) //0 for inactive, 1 for both players active, 2 for waiting for last player
		{
			string statement = $"UPDATE lobbies SET IsGameActive = {(int)newStatus} WHERE (LobbyID = {lobbyID}) LIMIT 1;";
			return ExecuteCommand(statement) > 0;
        }

		private bool RemovePlayerFromLobby(int playerID, int lobbyID)
		{
			int p1ID = GetPlayer1IDFromLobby(lobbyID);
			if (p1ID == playerID)
			{
                string statement = $"UPDATE lobbies SET Player1ID = 0 WHERE (LobbyID = {lobbyID}) LIMIT 1;";
                return ExecuteCommand(statement) > 0;
            }
			else
			{
                string statement = $"UPDATE lobbies SET Player2ID = 0 WHERE (LobbyID = {lobbyID}) LIMIT 1;";
                return ExecuteCommand(statement) > 0;
            }
        }

		private bool RemovePlayerLobbyNumber(int playerID)
		{
			string statement = $"UPDATE players SET LobbyNumber = 0 WHERE (PlayerID = {playerID}) LIMIT 1;";
			return ExecuteCommand(statement) > 0;
		}

		private bool RemoveLobby(int lobbyID)
		{
			string deleteLobby = $"DELETE FROM lobbies WHERE (LobbyID = {lobbyID}) LIMIT 1;";
			return ExecuteCommand(deleteLobby) > 0;
		}

		private bool Add1ToCurrentQuestion(int playerID)
		{
			string getCurrentQuestion = $"SELECT CurrentQuestion FROM `session stats` WHERE PlayerID = {playerID} LIMIT 1;";
			int currentQuestionNum = ExecuteQueryInt(getCurrentQuestion);
			if (currentQuestionNum <= 10)
			{
                string add1ToCurrentQuestionStatement = $"UPDATE `session stats` SET CurrentQuestion = {currentQuestionNum + 1} WHERE (PlayerID = {playerID}) LIMIT 1;";
                return ExecuteCommand(add1ToCurrentQuestionStatement) > 0;
            }
			else return true;
		}

        private bool AddScore(int playerID)
        {
			string getCurrentScore = $"SELECT Score FROM `session stats` WHERE PlayerID = {playerID} LIMIT 1;";
			int currentScore = ExecuteQueryInt(getCurrentScore);
			string addToCurrentScore = $"UPDATE `session stats` SET Score = {currentScore + 10} WHERE (PlayerID = {playerID}) LIMIT 1;";
			return ExecuteCommand(addToCurrentScore) > 0;
        }

        private bool TestTwoStatements(string statement1, string statement2)
		{
			bool test1 = ExecuteCommand(statement1) != 0;
			bool test2 = ExecuteCommand(statement2) > 0;
			return test1 & test2;
		}

		/// <summary>
		/// Connects this player to the server and adds them to the Players table.
		/// </summary>
		/// <param name="playerToken">Registers a unique token for the player to identify with</param>
		/// <param name="playerName">Registers this name as the player's alias</param>
		/// <returns>Success/Failure</returns>
		public bool AddNewPlayer(int playerToken, string playerName)
		{
			string statement = $"INSERT INTO players (PlayerName, PlayerToken) VALUES ('{playerName}', {playerToken}) LIMIT 1;";
			return ExecuteCommand(statement) > 0;
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
			if (GetPlayerLobby(playerID) != 0)
				playerLobby = GetPlayerLobby(playerID);
			else
				playerLobby = 0;
			if (playerLobby != 0)
			{
				int player1ID = GetPlayer1IDFromLobby(playerLobby);
				int player2ID = GetPlayer2IDFromLobby(playerLobby);
				if (player1ID == playerID) 
				{ 
					RemovePlayerStats(player1ID);
					RemovePlayerStats(player2ID);
					SubmitPlayerTicket(GetPlayerToken(player2ID));
					UpdatePlayerStatus(player2ID, PlayerStatus.Queue);
				}
				else 
				{
					RemovePlayerStats(player1ID);
					RemovePlayerStats(player2ID);
					SubmitPlayerTicket(GetPlayerToken(player1ID));
					UpdatePlayerStatus(player1ID, PlayerStatus.Queue);
				}
				RemoveLobby(playerLobby);
			}
			string deleteFromQueueQuery = $"DELETE FROM queue WHERE(PlayerID = {playerID}) LIMIT 1;";
			bool deleteFromQueue = ExecuteCommand(deleteFromQueueQuery) > 0;
			string deleteFromDBStatement = $"DELETE FROM players WHERE(PlayerToken = {playerToken}) LIMIT 1;";
			bool deleteFromDB = ExecuteCommand(deleteFromDBStatement) > 0;
			return deleteFromQueue & deleteFromDB;
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
			bool insertIntoQueue = ExecuteCommand(insertIntoQueueStatement) > 0;
			bool updateStatus = UpdatePlayerStatus(playerID, PlayerStatus.Queue);

			string getLFGPlayersInQueue = $"SELECT COUNT(queue.PlayerID) FROM queue INNER JOIN " +
				$"players ON queue.PlayerID = players.PlayerID WHERE AcceptMatch = 0 AND LobbyNumber = 0;";
			if (ExecuteQueryInt(getLFGPlayersInQueue) > 1)
			{
				string getPlayer2Query = $"SELECT queue.PlayerID FROM queue INNER JOIN players ON " +
					$"queue.PlayerID = players.PlayerID WHERE AcceptMatch = 0 AND LobbyNumber = 0 AND " +
					$"queue.PlayerID != {playerID} LIMIT 1;";
				int player2ID = ExecuteQueryInt(getPlayer2Query);
				int[] players = new int[2] { playerID, player2ID };
				CreateMatch(players);
			}
			return removeLobbyNumber & insertIntoQueue & updateStatus;
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
			if (GetPlayerLobby(playerID) != 0)
				playerLobby = GetPlayerLobby(playerID);
			else
				playerLobby = 0;
			if (playerLobby != 0)
			{
				int player1ID = GetPlayer1IDFromLobby(playerLobby);
				int player2ID = GetPlayer2IDFromLobby(playerLobby);
				if (player1ID == playerID) 
				{
					AcceptMatchTo0(player2ID);
					SubmitPlayerTicket(GetPlayerToken(player2ID));
					UpdatePlayerStatus(player2ID, PlayerStatus.Queue);
					RemovePlayerStats(player1ID);
                    RemovePlayerStats(player2ID);
                }
				else 
				{
                    AcceptMatchTo0(player1ID);
                    SubmitPlayerTicket(GetPlayerToken(player1ID));
					UpdatePlayerStatus(player1ID, PlayerStatus.Queue);
                    RemovePlayerStats(player1ID);
                    RemovePlayerStats(player2ID);
                }
				RemoveLobby(playerLobby);
			}
			string deleteFromQueueQuery = $"DELETE FROM queue WHERE(PlayerID = {playerID}) LIMIT 1;";
			bool deleteFromQueue = ExecuteCommand(deleteFromQueueQuery) > 0;
			bool updateStatus = UpdatePlayerStatus (playerID, 0);
			return deleteFromQueue & updateStatus;
		}

		public bool RemovePlayerTicketByID(int playerID)
		{
			string deleteFromQueue = $"DELETE FROM queue WHERE(PlayerID = {playerID}) LIMIT 1;";
			bool test1 = ExecuteCommand(deleteFromQueue) > 0;
			bool test2 = UpdatePlayerStatus(playerID, 0);
			return test1 & test2;
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
			bool updateQueueAccept = ExecuteCommand(statement) > 0;
			int lobbyID = GetPlayerLobby(playerID);
			if (GetHandshakeStatusFromLobby(lobbyID))
			{
				int p1ID = GetPlayer1IDFromLobby(lobbyID);
				int p2ID = GetPlayer2IDFromLobby(lobbyID);
                string insertP1StatsStatement = $"INSERT INTO `session stats` (PlayerID, LobbyID) VALUES ({p1ID}, {lobbyID});";
                string insertP2StatsStatement = $"INSERT INTO `session stats` (PlayerID, LobbyID) VALUES ({p2ID}, {lobbyID});";
				bool insertP1Stats = ExecuteCommand(insertP1StatsStatement) > 0;
				bool insertP2Stats = ExecuteCommand(insertP2StatsStatement) > 0;
                bool startMatch = StartMatch(lobbyID);
				return updateQueueAccept & insertP1Stats & insertP2Stats & startMatch;
			}
			else { return updateQueueAccept; }
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
			string getMatchStatus = $"SELECT IsGameActive FROM lobbies WHERE LobbyID = {lobbyID};";
            MatchStatus matchStatus = (MatchStatus)ExecuteQueryInt(getMatchStatus);
			if (matchStatus != MatchStatus.ActiveOnePlayer)
			{
                bool changeActiveStatus = ChangeGameActiveStatus(lobbyID, MatchStatus.ActiveOnePlayer);
                bool exitMatch =  ExitMatch(playerID);
				return changeActiveStatus & exitMatch;
			}
			else
			{ 
				bool exitMatch = ExitMatch(playerID); 
				bool endMatch = EndMatch(lobbyID);
				return exitMatch & endMatch;
			}
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
				if (answerID == ExecuteQueryInt(getCorrectAnswerQuery)) 
				{
					AddScore(playerID);
					Add1ToCurrentQuestion(playerID);
					return true; 
				}
				else 
				{
					Add1ToCurrentQuestion(playerID);
					return false;
				}
			}
			catch (Exception ex) // TODO State what exception you are catching
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
			string statement1 = $"INSERT INTO lobbies (Player1ID, Player2ID) VALUES ({playerIDs[0]}, {playerIDs[1]}) LIMIT 1;";
			bool insertIntoLobby = ExecuteCommand(statement1) > 0;
			int lobbyNumber = GetPlayerLobby(playerIDs[0]); //Creates and returns the lobby number
			string updateP1LobbyNum = $"UPDATE players SET PlayerStatus = {(int)PlayerStatus.Lobby}, LobbyNumber = {lobbyNumber} WHERE (PlayerID = {playerIDs[0]}) LIMIT 1;";
			string updateP2LobbyNum = $"UPDATE players SET PlayerStatus = {(int)PlayerStatus.Lobby}, LobbyNumber = {lobbyNumber} WHERE (PlayerID = {playerIDs[1]}) LIMIT 1;";
			return insertIntoLobby & TestTwoStatements(updateP1LobbyNum, updateP2LobbyNum);
		}

		/// <summary>
		/// Starts the match listed in MatchID
		/// </summary>
		/// <param name="matchID">Unique ID of the match to start</param>
		/// <returns>Success/Failure</returns>
		private bool StartMatch(int matchID) // TODO Remove accepting players from the waiting queue
		{
			//Get both players in the lobby. If both players signal 1 on queue - AcceptMatch, update both players' status to 2, and go through with the match
			//If 1 player leaves or doesn't handshake in time, use EndMatch(matchID) and use SubmitPlayerTicket(int playerToken) on the active player
			//Then use RemovePlayerTicket(int playerToken) on the inactive player
			int player1ID = GetPlayer1IDFromLobby(matchID);
			int player2ID = GetPlayer2IDFromLobby(matchID);
			bool updateGameStatus = ChangeGameActiveStatus(matchID, MatchStatus.ActiveBothPlayers);
			bool updateP1Status = UpdatePlayerStatus(player1ID, PlayerStatus.Lobby);
			bool updateP2Status = UpdatePlayerStatus(player2ID, PlayerStatus.Lobby);
			return updateGameStatus & updateP1Status & updateP2Status;
		}

		private bool ExitMatch(int playerID)
		{
			int lobbyID = GetPlayerLobby(playerID);
			bool removeStats = RemovePlayerStats(playerID);
            bool removeLobbyNum = RemovePlayerLobbyNumber(playerID);
			bool removePlayerFromLobby = RemovePlayerFromLobby(playerID, lobbyID);
            int playerToken = GetPlayerToken(playerID);
			bool removeTicket = RemovePlayerTicket(playerToken);
            return removeStats & removeLobbyNum & removePlayerFromLobby & removeTicket;
		}

		private bool EndMatch(int matchID)
		{
			bool removeLobby = RemoveLobby(matchID);
			return removeLobby;
		}
		#endregion
		#endregion
		#endregion

		#region ENUMS
		enum PlayerStatus
		{
			MainMenu = 0,
			Queue = 1,
			Lobby = 2
		}

		enum MatchStatus
		{
			Inactive = 0,
			ActiveBothPlayers = 1,
			ActiveOnePlayer = 2
		}
		#endregion
	}
}
