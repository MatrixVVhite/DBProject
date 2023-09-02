using Microsoft.AspNetCore.Mvc;
using Server.Database;
using JsonDict = System.Collections.Generic.Dictionary<string, object?>;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class QuestionController : ControllerBase
	{
		[HttpGet("{id}")]
		public JsonDict Get(int id)
		{
			return DatabaseManager.Instance.GetQuestion(id);
		}
	}
}
 