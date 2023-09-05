﻿using Microsoft.AspNetCore.Mvc;
using Server.Database;
using JsonDict = System.Collections.Generic.Dictionary<string, object?>;

namespace Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class GetNextQuestionController : ControllerBase
	{
		[HttpGet("{playerToken}")]
		public JsonDict Get(int playerToken)
		{
			return DatabaseManager.Instance.GetNextQuestionForPlayer(playerToken);
		}
	}
}
 