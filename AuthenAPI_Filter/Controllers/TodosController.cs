using AuthenAPI_CustomFilter.Filters;
using AuthenAPI_CustomFilter.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace AuthenAPI_CustomFilter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [TokenAuthenticationFilter]
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
