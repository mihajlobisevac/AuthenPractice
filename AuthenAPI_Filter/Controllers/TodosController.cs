using AuthenAPI_CustomJwt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace AuthenAPI_CustomJwt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TodosController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<Todo> Get()
        {
            var todos = new[]
            {
                new Todo { Id = 1, Title = "Todo 1", Description = "Do Todo 1" },
                new Todo { Id = 2, Title = "Todo 2", Description = "Do Todo 2" },
                new Todo { Id = 3, Title = "Todo 3", Description = "Do Todo 3" }
            };

            return todos;
        }
    }
}
