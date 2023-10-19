using Microsoft.AspNetCore.Mvc;
using Server.Database;

namespace Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ConnectToServerController : ControllerBase
	{
		[HttpPost]
		public int Post([FromForm] string playerName)
		{
			int playerToken = GetUniquePlayerToken();
			return DatabaseManager.Instance.AddNewPlayer(playerToken, playerName) ? playerToken : 0;
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
 