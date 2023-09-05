using Microsoft.AspNetCore.Mvc;
using Server.Database;

namespace Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AnswerQuestionController : ControllerBase
	{
		[HttpPost("{playerToken}&{answerID}")]
		public bool Post(int playerToken, int answerID)
		{
			return DatabaseManager.Instance.RegisterAnswer(playerToken, answerID);
		}
	}
}
 