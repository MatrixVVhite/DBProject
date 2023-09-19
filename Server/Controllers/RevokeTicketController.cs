using Microsoft.AspNetCore.Mvc;
using Server.Database;

namespace Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class RevokeTicketController : ControllerBase
	{
		[HttpPost]
		public bool Post([FromBody] int playerToken)
		{
			return DatabaseManager.Instance.RemovePlayerTicket(playerToken);
		}
	}
}
 