using AuthenAPI.Data;
using AuthenAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AuthenAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TodosController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public TodosController(ApiDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTodo(int id)
        {
            var item = await _context.Todos.FindAsync(id);

            if (item is null)
                return NotFound($"Not found todo ({id})");

            return Ok(item);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetTodos()
        {
            var items = await _context.Todos.ToListAsync();

            return Ok(items);
        }

        [HttpPost()]
        public async Task<IActionResult> CreateTodo(TodoItem newItem)
        {
            if (ModelState.IsValid)
            {
                await _context.Todos.AddAsync(newItem);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetTodo", new { newItem.Id }, newItem);
            }

            return new JsonResult("Something went wrong")
            {
                StatusCode = 500
            };
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodo(int id, TodoItem newItem)
        {
            if (id != newItem.Id)
                return BadRequest("Id mismatch");

            var item = await _context.Todos.FindAsync(id);

            if (item is null)
                return NotFound($"Not found todo ({id})");

            item.Title = newItem.Title;
            item.Completed = newItem.Completed;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(int id)
        {
            var item = await _context.Todos.FindAsync(id);

            if (item is null)
                return NotFound($"Not found todo ({id})");

            _context.Todos.Remove(item);

            await _context.SaveChangesAsync();

            return Ok(item);
        }
    }
}
