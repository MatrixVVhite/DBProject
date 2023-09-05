using Microsoft.AspNetCore.Mvc;
using Server.Database;

namespace Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class IsTicketValidController : ControllerBase
	{
		[HttpGet("{playerToken}")]
		public bool Get(int playerToken)
		{
			return DatabaseManager.Instance.GetTicketExists(playerToken);
		}
	}
}
 