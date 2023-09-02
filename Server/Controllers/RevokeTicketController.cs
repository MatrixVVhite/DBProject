using Microsoft.AspNetCore.Mvc;
using Server.Database;

namespace Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class RevokeTicketController : ControllerBase
	{
		[HttpPost("{playerToken}")]
		public bool Post(int playerToken)
		{
			if (DatabaseManager.Instance.GetTicketExists(playerToken))
				return DatabaseManager.Instance.RemovePlayerTicket(playerToken);
			else
				return true;
		}
	}
}
 