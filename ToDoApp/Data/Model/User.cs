using System.Text.Json.Serialization;

namespace ToDoApp.Data.Model
{
    public class User
    {
        public int Id { get; set; } 
        public string Name { get; set; }
        public string Email { get; set; }

        public byte[] HashPassword { get; set; }
        public byte[] HashSaltPassword { get; set; }

        [JsonIgnore]
        public List<Todo> Todos { get; set; }
        
    }
}
