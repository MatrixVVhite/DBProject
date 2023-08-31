using Microsoft.AspNetCore.Mvc;
using Server.Database;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Server.Controllers
{
	[Serializable]
	public class Question
	{
		public int ID { get; set; }
		public string QuestionText { get; set; }
		public string Answer1 { get; set; }
		public string Answer2 { get; set; }
		public string Answer3 { get; set; }
		public string Answer4 { get; set; }

		public Question(int id, string question, string answer1, string answer2, string answer3, string answer4)
		{
			ID = id;
			QuestionText = question;
			Answer1 = answer1;
			Answer2 = answer2;
			Answer3 = answer3;
			Answer4 = answer4;
		}
	}

	[Route("api/[controller]")]
	[ApiController]
	public class QuestionController : ControllerBase
	{
		// GET: api/<QuestionController>
		[HttpGet]
		public IEnumerable<string> Get()
		{
			return new string[] { "value1", "value2" };
		}

		// GET api/<QuestionController>/5s
		[HttpGet("{id}")]
		public string Get(int id)
		{
			var manager = new DatabaseManager();

			return manager.GetQuestionText(id);
		}
	}
}
 