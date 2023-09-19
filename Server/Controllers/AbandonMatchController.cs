using Microsoft.AspNetCore.Mvc;
using Server.Database;

namespace Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AbandonMatchController : ControllerBase
	{
		[HttpPost]
		public bool Post([FromForm] int playerToken)
		{
			return DatabaseManager.Instance.LeaveMatch(playerToken);
		}
	}
}
 