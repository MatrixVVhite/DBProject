﻿using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class RevokeTicketController : ControllerBase
	{
		// GET api/<SomethingController>/5s
		[HttpGet("{id}")]
		public void Get(int id)
		{
			throw new NotImplementedException();
		}
	}
}
 