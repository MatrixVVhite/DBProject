using Microsoft.AspNetCore.Mvc;
using Server.Database;

namespace Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class IsMatchFoundController : ControllerBase
	{
		[HttpGet("{playerToken}")]
		public (bool, int) Get(int playerToken)
		{
			return DatabaseManager.Instance.GetMatchFound(playerToken);
		}
	}
}
 