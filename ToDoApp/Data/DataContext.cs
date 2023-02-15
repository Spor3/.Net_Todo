using Microsoft.EntityFrameworkCore;
using ToDoApp.Data.Model;

namespace ToDoApp.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<User> User { get; set; }

        public DbSet<Todo> Todo { get; set; }
    }
}
