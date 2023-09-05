using Microsoft.AspNetCore.Mvc;
using Server.Database;

namespace Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class DisconnectFromServerController : ControllerBase
	{
		[HttpPost("{playerToken}")]
		public void Post(int playerToken)
		{
			DatabaseManager.Instance.RemovePlayer(playerToken);
		}
	}
}
 