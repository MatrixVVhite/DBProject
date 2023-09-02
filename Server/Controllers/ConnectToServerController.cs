using Microsoft.AspNetCore.Mvc;
using Server.Database;

namespace Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ConnectToServerController : ControllerBase
	{
		[HttpPost("{playerName}")]
		public int Post(string playerName)
		{
			int playerToken = GetUniquePlayerToken();
			DatabaseManager.Instance.AddNewPlayer(playerToken, playerName);
			return playerToken;
		}

		private int GetUniquePlayerToken()
		{
			int token;
			do
			{
				token = GenerateRandomToken();
			}
			while (GetTokenAlreadyExists(token));

			return token;
		}

		private int GenerateRandomToken() => Random.Shared.Next(int.MinValue, int.MaxValue);

		private bool GetTokenAlreadyExists(int token) => DatabaseManager.Instance.GetPlayerTokenExists(token);
	}
}
 