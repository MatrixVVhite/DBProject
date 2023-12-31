﻿using Microsoft.AspNetCore.Mvc;
using Server.Database;
using JsonDict = System.Collections.Generic.Dictionary<string, object?>;

namespace Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class GetMatchStatusController : ControllerBase
	{
		[HttpGet]
		public JsonDict Get(int matchID, int playerToken)
		{
			return DatabaseManager.Instance.GetMatchStatus(matchID, playerToken);
		}
	}
}
 