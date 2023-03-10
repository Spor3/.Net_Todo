using System.Text.Json.Serialization;

namespace ToDoApp.Data.Model
{
    public class Todo
    {
        public int Id { get; set; } 
        public string Title { get; set; }
        public string Description { get; set; }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        [JsonIgnore]
        public User User { get; set; }  
        public int UserId { get; set; }

    }
}
