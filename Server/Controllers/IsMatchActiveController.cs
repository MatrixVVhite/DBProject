using Microsoft.AspNetCore.Mvc;
using Server.Database;

namespace Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class IsMatchActiveController : ControllerBase
	{
		[HttpGet("{playerToken}")]
		public bool Get(int playerToken)
		{
			return DatabaseManager.Instance.GetMatchActive(playerToken);
		}
	}
}
 