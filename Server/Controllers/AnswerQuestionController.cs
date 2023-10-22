using Microsoft.AspNetCore.Mvc;
using Server.Database;
using JsonDict = System.Collections.Generic.Dictionary<string, object?>;

namespace Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AnswerQuestionController : ControllerBase
	{
		[HttpPost]
		public JsonDict Post([FromForm] int playerToken, [FromForm] int answerID, [FromForm] float answerTime)
		{
			return DatabaseManager.Instance.RegisterAnswer(playerToken, answerID, answerTime);
		}
	}
}
 