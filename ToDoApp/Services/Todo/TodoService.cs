using ErrorOr;
using Microsoft.EntityFrameworkCore;
using ToDoApp.Data;
using ToDoApp.Data.Model;
using ToDoApp.ServiceError;

namespace ToDoApp.Services.TodoService
{
    public class TodoService : ITodoService
    {
        private readonly DataContext _dataContext;

        public TodoService(DataContext dataContext)
        { 
            _dataContext = dataContext; 
        } 

        public async Task<ErrorOr<Created>> CreateUser(User user) 
        { 
            _dataContext.User.Add(user);
            await _dataContext.SaveChangesAsync();

            return Result.Created;
        }
        public async Task<ErrorOr<Updated>> UpdateUser(User user)
        {
            var upsertUser = await _dataContext.User.FindAsync(user.Id);

            if (upsertUser == null)
            {//Todo : Tornare il giusto errore
                return Errors.Todo.NotFound;
            }
            upsertUser.Name = user.Name;
            upsertUser.Email = user.Email;
            await _dataContext.SaveChangesAsync();

            return Result.Updated;
        }
        public async Task<ErrorOr<Deleted>> DeleteUser(int userId)
        {
            var user = await _dataContext.User.FindAsync(userId);
            if (user == null)
            {//Todo : Tornare giusto errore
                return Errors.Todo.NotFound;
            }
            _dataContext.User.Remove(user);
            await _dataContext.SaveChangesAsync();

            return Result.Deleted;
        }
        public async Task<ErrorOr<Todo>> CreateTodo(int userId,Todo todo)
        {
            var user = await _dataContext.User.FindAsync(userId);
            if (user == null)
            {
                return Errors.Todo.NotFound;
            }
            todo.User = user;
                _dataContext.Todo.Add(todo);    
                await _dataContext.SaveChangesAsync();
            return todo;

        }
        public async Task<ErrorOr<Todo>> UpdateTodo(int todoId, Todo todo)
        {
            var upsertTodo = await _dataContext.Todo.FindAsync(todoId);

            if(upsertTodo != null)
            {
                upsertTodo.Title = todo.Title;
                upsertTodo.Description = todo.Description;
                upsertTodo.Start = todo.Start;
                upsertTodo.End = todo.End;
                await _dataContext.SaveChangesAsync();

                return upsertTodo;
            }
            return Errors.Todo.NotFound;
        }
        public async Task<ErrorOr<Deleted>>  DeleteTodo(int todoId)
        {
            var todo = await _dataContext.Todo.FindAsync(todoId);
            if(todo == null)
            {
                return Errors.Todo.NotFound;
            }

            _dataContext.Todo.Remove(todo);
            await _dataContext.SaveChangesAsync();
            return Result.Deleted;

        }
        public async Task<ErrorOr<List<Todo>>> GetTodosByUserId(int userId)
        {
            var todosList = await _dataContext.Todo.Where(c => c.UserId == userId).ToListAsync();
            
            return todosList;
        }

        public async Task<ErrorOr<Todo>> GetSingleTodo(int userId, int todoId)
        {
            var todosListByUser = await _dataContext.Todo.Where(c => c.UserId == userId).ToListAsync();

                var todo = todosListByUser.Find(c => c.Id == todoId);
                if(todo == null)
                {
                    return Errors.Todo.NotFound;
                }
                return todo;
        }
    } 
}
