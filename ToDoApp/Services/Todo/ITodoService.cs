using ErrorOr;
using ToDoApp.Data.Model;

namespace ToDoApp.Services.TodoService
{
    public interface ITodoService
    {

        Task<ErrorOr<Todo>> CreateTodo(int userId, Todo todo);
        Task<ErrorOr<Todo>> UpdateTodo(int userId,int todoId, Todo todo);
        Task<ErrorOr<Deleted>> DeleteTodo(int userId,int id);
        Task<ErrorOr<List<Todo>>> GetTodosByUserId(int userId);
        Task<ErrorOr<Todo>> GetSingleTodo(int userId,int todoId);
    }
}
