using Microsoft.AspNetCore.Mvc;
using Server.Database;

namespace Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AbandonMatchController : ControllerBase
	{
		[HttpPost("{playerToken}")]
		public bool Post(int playerToken)
		{
			return DatabaseManager.Instance.LeaveMatch(playerToken);
		}
	}
}
 