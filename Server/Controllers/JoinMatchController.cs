using Microsoft.AspNetCore.Mvc;
using Server.Database;

namespace Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class JoinMatchController : ControllerBase
	{
		[HttpPost("{playerToken}&{matchID}")]
		public bool Post(int playerToken, int matchID)
		{
			return DatabaseManager.Instance.JoinMatch(playerToken, matchID);
		}
	}
}
 