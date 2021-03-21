using AuthenAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthenAPI.Data
{
    public class ApiDbContext : IdentityDbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options)
            :base(options)
        {
        }

        public DbSet<TodoItem> Todos { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}
