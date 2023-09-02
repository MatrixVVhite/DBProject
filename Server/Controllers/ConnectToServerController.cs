using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ConnectToServerController : ControllerBase
	{
		// GET api/<SomethingController>/5s
		[HttpGet("{id}")]
		public void Get()
		{
			throw new NotImplementedException();
		}
	}
}
 