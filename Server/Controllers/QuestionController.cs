using Microsoft.AspNetCore.Mvc;
using Server.Database;
using Server.DataStructures;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class QuestionController : ControllerBase
	{
		// GET api/<QuestionController>/5s
		[HttpGet("{id}")]
		public Question Get(int id)
		{
			var manager = new DatabaseManager();

			return manager.GetQuestion(id);
		}
	}
}
 