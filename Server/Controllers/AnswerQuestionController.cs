using Microsoft.AspNetCore.Mvc;
using Server.Database;

namespace Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AnswerQuestionController : ControllerBase
	{
		[HttpPost]
		public bool Post([FromForm] int playerToken, [FromForm] int answerID)
		{
			return DatabaseManager.Instance.RegisterAnswer(playerToken, answerID);
		}
	}
}
 