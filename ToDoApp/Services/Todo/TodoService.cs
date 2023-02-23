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


        public async Task<ErrorOr<Todo>> CreateTodo(int userId,Todo todo)
        {
            var user = await _dataContext.User.FindAsync(userId);
            if (user == null)
            {
                return Errors.User.NotFound;
            }
            todo.User = user;
                _dataContext.Todo.Add(todo);    
                await _dataContext.SaveChangesAsync();
            return todo;

        }
        public async Task<ErrorOr<Todo>> UpdateTodo(int userId,int todoId, Todo todo)
        {
            var listUpsertTodo =  _dataContext.Todo.Where(c => c.UserId == userId).AsQueryable();
            var upsertTodo = await listUpsertTodo.Where(c => c.Id == todoId).FirstOrDefaultAsync();

            if(upsertTodo != null)
            {
                upsertTodo.Title = todo.Title == null ? upsertTodo.Title : todo.Title;
                upsertTodo.Description = todo.Description == null ? upsertTodo.Description : todo.Description;
                upsertTodo.Start = todo.Start.ToString().Length == 0 ? upsertTodo.Start : todo.Start;
                upsertTodo.End = todo.End.ToString().Length == 0 ? upsertTodo.End : todo.End;
                await _dataContext.SaveChangesAsync();

                return upsertTodo;
            }
            return Errors.Todo.NotFound;
        }
        public async Task<ErrorOr<Deleted>>  DeleteTodo(int userId, int todoId)
        {
            var listDeleteTodo = _dataContext.Todo.Where(c => c.UserId == userId).AsQueryable();
            var todo =  await listDeleteTodo.Where(c => c.Id == todoId).FirstOrDefaultAsync();
            if (todo == null)
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
