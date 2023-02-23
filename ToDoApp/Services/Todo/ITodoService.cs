using ErrorOr;
using ToDoApp.Data.Model;

namespace ToDoApp.Services.TodoService
{
    public interface ITodoService
    {
        Task<ErrorOr<Created>> CreateUser(User user);
        Task<ErrorOr<User>> TakePss(User user);
        Task<ErrorOr<Updated>> UpdateUser(User user);
        Task<ErrorOr<Deleted>> DeleteUser(int userId);

        Task<ErrorOr<Todo>> CreateTodo(int userId, Todo todo);
        Task<ErrorOr<Todo>> UpdateTodo(int todoId, Todo todo);
        Task<ErrorOr<Deleted>> DeleteTodo(int id);
        Task<ErrorOr<List<Todo>>> GetTodosByUserId(int userId);
        Task<ErrorOr<Todo>> GetSingleTodo(int userId,int todoId);
    }
}
